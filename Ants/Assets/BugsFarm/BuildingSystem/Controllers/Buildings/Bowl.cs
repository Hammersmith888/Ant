using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Quest;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Bowl : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly GetResourceSystem _getResourceSystem;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingSceneObjectStorage _viewStorageStorage;
        private readonly BuildingDtoStorage _dtoStorage;
        private readonly IInstantiator _instantiator;

        private Type GetTaskType => typeof(GetResourceBootstrapTask);
        private const string _itemId = "4"; // water item
        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const AnimKey _actionAnimKey = AnimKey.Drink;

        private StatsCollection _statCollection;
        private IInventory _inventory;
        private IDisposable _upgradeEvent;
        private StatVital _resourceStat;
        private BowlSceneObject _view;
        private ResourceInfo _resourceInfo;

        public Bowl(string guid,
                    GetResourceSystem getResourceSystem,
                    StatsCollectionStorage statsCollectionStorage,
                    BuildingSceneObjectStorage viewStorageStorage,
                    BuildingDtoStorage dtoStorage,
                    IInstantiator instantiator)
        {
            _getResourceSystem = getResourceSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _viewStorageStorage = viewStorageStorage;
            _dtoStorage = dtoStorage;
            _instantiator = instantiator;
            Id = guid;
        }

        public void Initialize()
        {
            // статы
            _statCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statCollection.Get<StatVital>(_resourceStatKey);
            _resourceStat.OnValueChanged += OnResourceValueChanged;
            
            var slot = new ItemSlot(_itemId, (int) _resourceStat.CurrentValue, (int) _resourceStat.Value);
            var invetoryProtocol = new CreateInventoryProtocol(Id, res=>_inventory=res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[]{Id});
            _resourceInfo.OnUpdate += OnResourceInfoUpdate;

            _view = (BowlSceneObject) _viewStorageStorage.Get(Id);
            _getResourceSystem.OnResourceChanged += OnResourceChanged;
            _getResourceSystem.OnResourceDepleted += OnResourceDepleted;
            Production();
            UpdateWaterSize();
        }
        
        public void Dispose()
        {
            _resourceStat.OnValueChanged -= OnResourceValueChanged;
            _resourceStat = null;
            _getResourceSystem.OnResourceChanged -= OnResourceChanged;
            _getResourceSystem.OnResourceDepleted -= OnResourceDepleted;
            _getResourceSystem.UnRegistration(_itemId, Id);
            _resourceInfo.OnUpdate -= OnResourceInfoUpdate;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        private void OnResourceDepleted(string itemId, string guid)
        {
            if (Id != guid)
            {
                return;
            }

            _getResourceSystem.UnRegistration(itemId, guid);
            UpdateWaterSize();
        }

        private void OnResourceChanged(string itemId, string guid)
        {
            if (Id != guid)
            {
                return;
            }

            Production();
            UpdateWaterSize();
        }

        private void Production()
        {
            if (!_getResourceSystem.Contains(_itemId, Id) && _inventory.HasItem(_itemId))
            {
                var dto = _dtoStorage.Get(Id);
                var taskPoints = _view.GetComponent<TasksPoints>().Points;
                var args = ResourceArgs.Default().SetActionAnimKeys(_actionAnimKey);
                var maxUnits = (int) _statCollection.GetValue(_maxUnitsStatKey);
                var resourceProtocol = new GetResourceProtocol(Id, dto.ModelID, _itemId, maxUnits,
                                                               taskPoints, GetTaskType, args);
                _getResourceSystem.Registration(resourceProtocol);
            }
        }

        private void UpdateWaterSize()
        {
            if (!_view.WaterRenderer)
            {
                return;
            }
            
            var itemSlot = _inventory.GetItemSlot(_itemId);
            var currProgerss = (float) itemSlot.Count / itemSlot.Capacity;
            var currWaterSize = currProgerss * _view.InitWaterSize;
            var size = _view.WaterRenderer.size.SetY(currWaterSize);
            _view.WaterRenderer.size = size;
        }

        private void OnResourceInfoUpdate()
        {
            var itemSlot = _inventory.GetItemSlot(_itemId);
            _resourceInfo.SetInfo(Format.Resource(itemSlot.Count, itemSlot.Capacity));
        }
        
        private void OnResourceValueChanged(object sender, EventArgs e)
        {
            _inventory?.SetSlotCapacity(_itemId, (int)_statCollection.GetValue(_resourceStatKey));
        }
    }
}