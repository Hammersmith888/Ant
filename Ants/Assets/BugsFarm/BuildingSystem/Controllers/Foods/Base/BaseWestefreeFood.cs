using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseWestefreeFood : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }
        protected virtual string ItemId => "0";                   // предмет еды по умолчанию
        protected virtual AnimKey TakeAnim => AnimKey.TakeFoodHi; // действие получить ресурс
        protected virtual AnimKey WalkAnim => AnimKey.Walk;       // действие получить ресурс
        protected virtual Type TaskType => typeof(GetResourceBootstrapTask);
        
        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        
        private GetResourceSystem _getResourceSystem;
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingSceneObjectStorage _viewStorage;
        private BuildingDtoStorage _dtoStorage;
        private IInstantiator _instantiator;
        private IInventory _inventory;
        
        private StatsCollection _statCollection;
        private ResourceInfo _resourceInfo;
        private StatVital _resourceStat;
        private bool _finalized;

    #region Zenject

        [Inject]
        private void Inject(string guid,
                            IInstantiator instantiator,
                            StatsCollectionStorage statsCollectionStorage,
                            BuildingSceneObjectStorage viewStorage,
                            BuildingDtoStorage dtoStorage,
                            GetResourceSystem getResourceSystem)
        {
            Id = guid;
            _instantiator = instantiator;
            _getResourceSystem = getResourceSystem;
            _statsCollectionStorage = statsCollectionStorage;
            _viewStorage = viewStorage;
            _dtoStorage = dtoStorage;
        }

        public void Initialize()
        {
            // статы
            _statCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statCollection.Get<StatVital>(_resourceStatKey);
            _resourceStat.OnValueChanged += OnResourceValueChanged;

            // инвентарь
            var slot = new ItemSlot(ItemId, (int)_resourceStat.CurrentValue, (int)_resourceStat.Value);
            var invetoryProtocol = new CreateInventoryProtocol(Id, res => _inventory = res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);
            
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[]{Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;
            _getResourceSystem.OnResourceChanged += OnResourceChanged;
            _getResourceSystem.OnResourceDepleted += OnResourceDepleted;

            Production(ItemId);
        }

        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            _resourceStat.OnValueChanged -= OnResourceValueChanged;
            _resourceStat = null;
            _statCollection = null;
                
            _getResourceSystem.OnResourceChanged -= OnResourceChanged;
            _getResourceSystem.OnResourceDepleted -= OnResourceDepleted;
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;

            _getResourceSystem.UnRegistration(ItemId, Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

    #endregion

    #region Production

        private void Production(string itemId)
        {
            if(_finalized) return;
            if (!_inventory.HasItem(itemId) || _getResourceSystem.Contains(itemId, Id))
            {
                return;
            }

            var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
            var maxUnits = (int) _statCollection.GetValue(_maxUnitsStatKey);
            var dto = _dtoStorage.Get(Id);
            var args = ResourceArgs.Default()
                .SetActionAnimKeys(TakeAnim)
                .SetWalkAnimKeys(WalkAnim);
            var resourceProtocol = new GetResourceProtocol(Id, dto.ModelID, itemId, maxUnits,
                                                           taskPoints, TaskType, args);
            _getResourceSystem.Registration(resourceProtocol);
            UpdateStage(itemId);
        }

        private void UpdateStage(string itemId)
        {
            if(_finalized) return;
            var itemSlot = _inventory.GetItemSlot(itemId);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

    #endregion

    #region Resource Systems
        
        private void OnResourceDepleted(string itemId, string guid)
        {
            if (Id != guid || _finalized)
            {
                return;
            }

            if (!_getResourceSystem.Contains(itemId, guid))
            {
                return;
            }

            _getResourceSystem.UnRegistration(itemId, guid);
            UpdateStage(itemId);
        }

        private void OnResourceChanged(string itemId, string guid)
        {
            if (Id != guid || _finalized)
            {
                return;
            }

            UpdateStage(itemId);
        }

        private void OnUpdateResourceInfo()
        {
            if (_finalized) return;
            var itemSlot = _inventory.GetItemSlot(ItemId);
            _resourceInfo.SetInfo(Format.Resource(itemSlot.Count, itemSlot.Capacity));
        }
        
        private void OnResourceValueChanged(object sender, EventArgs e)
        {
            _inventory?.SetSlotCapacity(ItemId, (int)_statCollection.GetValue(_resourceStatKey));
        }

    #endregion
    }
}