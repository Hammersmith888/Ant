using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Quest;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Garden : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly IInstantiator _instantiator;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingDtoStorage _dtoStorage;

        private const string _growTimeTextKey = "BuildingInner_43_GrowthTime";
        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _growTimeStatKey = "stat_growTime";

        private AnimKey GetActionAnimKey => AnimKey.TakeFoodMid;
        private AnimKey GetWalkAnimKey => AnimKey.Walk;
        private string ItemID => "0"; // food item
        private Type TaskType => typeof(GetResourceBootstrapTask);

        private IEnumerable<IPosSide> _taskPoints;
        private IInventory _inventory;
        private ResourceInfo _resourceInfo;
        private StateInfo _stateInfo;
        private StatsCollection _statsCollection;
        private StatVital _resourceStat;
        private StatVital _growTimeStat;
        private ITask _growingTask;
        private bool _finalized;

        public Garden(string guid,
                      IInstantiator instantiator,
                      IBuildingBuildSystem buildingBuildSystem,
                      GetResourceSystem getResourceSystem,
                      StatsCollectionStorage statsCollectionStorage,
                      BuildingSceneObjectStorage viewStorage,
                      BuildingDtoStorage dtoStorage)
        {
            _getResourceSystem = getResourceSystem;
            _instantiator = instantiator;
            _buildingBuildSystem = buildingBuildSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _viewStorage = viewStorage;
            _dtoStorage = dtoStorage;
            Id = guid;
        }

        public void Initialize()
        {
            // статы
            _statsCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _growTimeStat = _statsCollection.Get<StatVital>(_growTimeStatKey);
            _resourceStat.OnValueChanged += OnResourceValueChanged;
            // инвентарь
            var slot = new ItemSlot(ItemID, (int)_resourceStat.CurrentValue, (int) _resourceStat.Value);
            var invetoryProtocol = new CreateInventoryProtocol(Id, res => _inventory = res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);

            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[] {Id});
            _stateInfo = _instantiator.Instantiate<StateInfo>(new object[] {Id});
            _stateInfo.OnUpdate += OnStateInfoUpdate;
            _resourceInfo.OnUpdate += OnResourceInfoUpdate;

            var view = (BuildingSpineObject) _viewStorage.Get(Id);
            var animatorProtocol = new CreateAnimatorProtocol(Id, GetType().Name, view.MainSkeleton);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animatorProtocol);
            _taskPoints = view.GetComponent<TasksPoints>().Points;
            
            _buildingBuildSystem.OnCompleted += OnBuildingCompleted;
            _buildingBuildSystem.OnStarted += OnBuildingStarted;
            _buildingBuildSystem.Registration(Id);

            _getResourceSystem.OnResourceDepleted += OnResourceDepleted;

            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
                UpdateStage();
            }
            else
            {
                Production();
            }
        }

        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            _growingTask?.Interrupt();
            _growingTask = null;

            _resourceInfo.OnUpdate -= OnResourceInfoUpdate;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            
            _stateInfo.OnUpdate -= OnStateInfoUpdate;
            _stateInfo.Dispose();
            _stateInfo = null;

            _statsCollection = null;
            _resourceStat.OnValueChanged -= OnResourceValueChanged;
            _resourceStat = null;
            _growTimeStat = null;
            
            _buildingBuildSystem.OnStarted -= OnBuildingStarted;
            _buildingBuildSystem.OnCompleted -= OnBuildingCompleted;
            _getResourceSystem.OnResourceDepleted -= OnResourceDepleted;

            _buildingBuildSystem.UnRegistration(Id);
            _getResourceSystem.UnRegistration(ItemID, Id);
            
            _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(new RemoveAnimatorProtocol(Id));
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        private void Production()
        {
            if(_finalized) return;
            if (!_inventory.HasItem(ItemID) && _growingTask == null && _growTimeStat.CurrentValue > 0)
            {
                var args = new object[] {Id, _taskPoints};
                _growingTask = _instantiator.Instantiate<GardenGrowingTask>(args);
                _growingTask.OnComplete  += OnGrowingComplete;
                _growingTask.OnInterrupt += OnGrowingInterrupt;
                _growingTask.Execute();
                UpdateStage();
                return;
            }

            if (_inventory.HasItem(ItemID) && !_getResourceSystem.Contains(ItemID, Id))
            {
                _growingTask?.Interrupt();
                _growingTask = null;

                var maxUnits = (int) _statsCollection.GetValue(_maxUnitsStatKey);
                var dto = _dtoStorage.Get(Id);
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(GetActionAnimKey)
                    .SetWalkAnimKeys(GetWalkAnimKey);
                var protocol = new GetResourceProtocol(Id, dto.ModelID, ItemID, maxUnits, 
                                                       _taskPoints, TaskType, args);

                _getResourceSystem.Registration(protocol);
                UpdateStage(true);
            }
        }

        private void UpdateStage(bool forceFull = false)
        {
            if(_finalized) return;
            var itemSlot = _inventory.GetItemSlot(ItemID);
            var current = forceFull ? itemSlot.Capacity : itemSlot.Count;
            var protocol = new SetStageBuildingProtocol(Id, current, itemSlot.Capacity);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        private void OnGrowingInterrupt(ITask task)
        {
            _growingTask = null;
            if(_finalized || _buildingBuildSystem.IsBuilding(Id)) return;
            Production();
        }

        private void OnGrowingComplete(ITask task)
        {
            if(_finalized || _growingTask == null || _buildingBuildSystem.IsBuilding(Id)) return;

            var listItems = new List<IItem>();
            var protocol = new CreateItemProtocol(ItemID, (int) _resourceStat.Value, listItems);
            _instantiator.Instantiate<CreateItemCommand>().Execute(protocol);
            _inventory.AddItems(listItems);
            _growingTask = null;
            Production();
        }

        private void OnBuildingStarted(string guid)
        {
            if(_finalized) return;
            if (guid != Id) return;
            _growingTask?.Interrupt();
            _growingTask = null;
        }

        private void OnBuildingCompleted(string guid)
        {
            if(_finalized) return;
            if (guid != Id) return;
            _growingTask?.Interrupt();
            _growingTask = null;
            Production();
        }

        private void OnResourceDepleted(string itemId, string guid)
        {
            if(_finalized) return;
            if (guid != Id || ItemID != itemId || !_getResourceSystem.Contains(ItemID, Id))
            {
                return;
            }

            MessageBroker.Default.Publish(new QuestUpdateProtocol()
            {
                QuestType = QuestType.CollectResource,
                ReferenceID = _dtoStorage.Get(Id).ModelID,
                Value = 1
            });
            
            _getResourceSystem.UnRegistration(ItemID, Id);
            _growTimeStat.SetMax();
            Production();
        }

        private void OnResourceInfoUpdate()
        {
            if(_finalized) return;
            var slot = _inventory.GetItemSlot(ItemID);
            _resourceInfo.SetInfo(Format.Resource(slot.Count, slot.Capacity));
        }
        
        private void OnStateInfoUpdate()
        {
            if(_finalized || _buildingBuildSystem.IsBuilding(Id)) return;
  
            if (_growTimeStat.CurrentValue > 0)
            {
                var text = LocalizationManager.Localize(_growTimeTextKey) + " ";
                _stateInfo.SetInfo(text + Format.Age(Format.MinutesToSeconds(_growTimeStat.CurrentValue)));
                return;
            }
            _stateInfo.SetInfo("");
        }
        
        private void OnResourceValueChanged(object sender, EventArgs e)
        {
            _inventory?.SetSlotCapacity(ItemID, (int)_statsCollection.GetValue(_resourceStatKey));
        }
    }
}