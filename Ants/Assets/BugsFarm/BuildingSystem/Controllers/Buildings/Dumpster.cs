using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Dumpster : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private string ItemID => "3"; // предмет мусора
        private AnimKey AddActionAnimKey => AnimKey.GarbageDropRecycler;
        private AnimKey AddWalkAnimKey => AnimKey.WalkGarbage;
        private Type TaskType => typeof(AddDumpsterTask);

        private readonly AddResourceSystem _addResourceSystem;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingDtoStorage _dtoStorage;
        private readonly IInstantiator _instantiator;

        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _recyclingTimeStatKey = "stat_recyclingTime";

        private bool _finalized;
        private IInventory _inventory;
        private ResourceInfo _resourceInfo;
        private StatsCollection _statsCollection;
        private StatVital _resourceStat;
        private ITask _recylingTask;

        public Dumpster(string guid,
                        AddResourceSystem addResourceSystem,
                        IBuildingBuildSystem buildingBuildSystem,
                        StatsCollectionStorage statsCollectionStorage,
                        BuildingSceneObjectStorage viewStorage,
                        BuildingDtoStorage dtoStorage,
                        IInstantiator instantiator)
        {
            Id = guid;
            _addResourceSystem = addResourceSystem;
            _buildingBuildSystem = buildingBuildSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _instantiator = instantiator;
            _viewStorage = viewStorage;
            _dtoStorage = dtoStorage;
        }

        public void Initialize()
        {
            _statsCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _resourceStat.OnValueChanged += OnResourceValueChanged;
            
            var slot = new ItemSlot(ItemID, (int)_resourceStat.CurrentValue, (int) _resourceStat.Value);
            var inventoryProtocol = new CreateInventoryProtocol(Id, res=>_inventory=res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            _addResourceSystem.OnResourceFull += OnResourceFull;
            _addResourceSystem.OnResourceChanged += OnResourceAdd;

            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[]{Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;

            _buildingBuildSystem.OnCompleted += OnBuildingCompleted;
            _buildingBuildSystem.OnStarted += OnBuildingStarted;
            _buildingBuildSystem.Registration(Id);

            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
            }
            else
            {
                Production();
            }
        }

        public void Dispose()
        {
            if (_finalized) return;
            _finalized = true;

            if (_recylingTask != null)
            {
                _recylingTask.Interrupt();
                _recylingTask = null;
            }

            _resourceStat.OnValueChanged -= OnResourceValueChanged;
            _resourceStat = null;
            
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            _addResourceSystem.OnResourceFull -= OnResourceFull;
            _addResourceSystem.OnResourceChanged -= OnResourceAdd;
            _buildingBuildSystem.UnRegistration(Id);
            _addResourceSystem.UnRegistration(ItemID, Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        private void Production()
        {
            if (_finalized) return;
            var itemSlot = _inventory.GetItemSlot(ItemID);
            var needAdd = itemSlot.Count < itemSlot.Capacity;

            if (_recylingTask == null)
            {
                _recylingTask = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
                _recylingTask.OnComplete += OnRecylingComplete;
                _recylingTask.Execute(Id, _recyclingTimeStatKey);
            }

            if (needAdd && !_addResourceSystem.Contains(ItemID, Id))
            {
                var maxUnits = (int) _statsCollection.GetValue(_maxUnitsStatKey);
                var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
                var dto = _dtoStorage.Get(Id);
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AddActionAnimKey)
                    .SetWalkAnimKeys(AddWalkAnimKey);
                var protocol = new AddResourceProtocol(Id,
                                                       dto.ModelID,
                                                       ItemID,
                                                       maxUnits,
                                                       taskPoints,
                                                       TaskType,
                                                       args,
                                                       false);
                _addResourceSystem.Registration(protocol);
            }
            else if (!needAdd && _addResourceSystem.Contains(ItemID, Id))
            {
                _addResourceSystem.UnRegistration(ItemID, Id);
            }
        }

        private void UpdateStage()
        {
            if (_finalized) return;

            var itemSlot = _inventory.GetItemSlot(ItemID);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        private void OnBuildingStarted(string guid)
        {
            if (_finalized || guid != Id) return;

            if (_addResourceSystem.Contains(ItemID, Id))
            {
                _addResourceSystem.UnRegistration(ItemID, Id);
            }

            _recylingTask?.Interrupt();
            _recylingTask = null;
        }

        private void OnBuildingCompleted(string guid)
        {
            if (_finalized || guid != Id) return;
            Production();
        }

        private void OnRecylingComplete(ITask task)
        {
            if (_finalized) return;
            _recylingTask = null;
            if (_buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }

            if (_inventory.HasItem(ItemID))
            {
                _inventory.Remove(ItemID, 1);
            }

            var recyclingTimeStat = _statsCollection.Get<StatVital>(_recyclingTimeStatKey);
            recyclingTimeStat.SetMax();
            UpdateStage();
            Production();
        }

        private void OnResourceAdd(string itemId, string guid)
        {
            if (_finalized) return;
            if (_buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }

            if (guid != Id || itemId != ItemID || _finalized)
            {
                return;
            }

            UpdateStage();
            Production();
        }

        private void OnResourceFull(string itemId, string guid)
        {
            if (_finalized) return;
            if (_buildingBuildSystem.IsBuilding(Id))
            {
                return;
            }

            if (guid != Id || itemId != ItemID || _finalized)
            {
                return;
            }
            UpdateStage();
            Production();
        }

        private void OnUpdateResourceInfo()
        {
            if (_finalized)  return;

            var slot = _inventory.GetItemSlot(ItemID);
            _resourceInfo.SetInfo(Format.Resource(slot.Count, slot.Capacity));
        }
        
        private void OnResourceValueChanged(object sender , EventArgs e)
        {
            _inventory?.SetSlotCapacity(ItemID, (int)_statsCollection.GetValue(_resourceStatKey));
        }
    }
}