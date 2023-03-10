using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseTrashFood : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }
        protected virtual string ItemID => "0"; // предмет еды по умолчанию
        protected virtual string GrabageItemId => "3"; // предмет мусора по умолчанию
        protected virtual AnimKey TakeAnim => AnimKey.TakeFoodMid; // действие получить ресурс
        protected virtual AnimKey WalkAnim => AnimKey.Walk; // идти к ресурсу
        protected virtual AnimKey TakeGrabageAnim => AnimKey.TakeGarbage; // действие получить ресурс
        protected virtual Type TaskType => typeof(GetResourceBootstrapTask);

        private GetResourceSystem _getResourceSystem;
        private IInstantiator _instantiator;
        private IInventory _inventory;
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingSceneObjectStorage _viewStorage;
        private BuildingDtoStorage _dtoStorage;
        
        private const string _grabageTextKey = "Grabage";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _prodResourceStatKey = "stat_maxResource";
        private const string _grabResourceStatKey  = "stat_maxGrabage";

        private StatsCollection _statsCollection;
        private ResourceInfo _resourceInfo;
        private StatVital _prodResourceStat;
        private bool _finalized;

        #region Zenject

        [Inject]
        private void Inject(IInstantiator instantiator,
                            StatsCollectionStorage statsCollectionStorage,
                            BuildingSceneObjectStorage viewStorage,
                            BuildingDtoStorage dtoStorage,
                            GetResourceSystem getResourceSystem,
                            string guid)
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
            _statsCollection = _statsCollectionStorage.Get(Id);
            _prodResourceStat = _statsCollection.Get<StatVital>(_prodResourceStatKey);
            var grabResourceStat = _statsCollection.Get<StatVital>(_grabResourceStatKey);
            _prodResourceStat.OnValueChanged += OnResourceValueChanged;
            // инвентарь
            var items = new[]
            {
                new ItemSlot(ItemID, (int)_prodResourceStat.CurrentValue, (int)_prodResourceStat.Value),
                new ItemSlot(GrabageItemId, (int)grabResourceStat.CurrentValue, (int)grabResourceStat.Value),
            };
            var invetoryProtocol = new CreateInventoryProtocol(Id, res=>_inventory=res, items);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);
            
            // инфо
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[]{Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;
            
            _getResourceSystem.OnResourceChanged     += OnResourceChanged;
            _getResourceSystem.OnResourceDepleted    += OnResourceChanged;

            Production();
        }
        
        public void Dispose()
        {
            _finalized = true;
            _getResourceSystem.OnResourceChanged -= OnResourceChanged;
            _getResourceSystem.OnResourceDepleted -= OnResourceChanged;
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            
            _prodResourceStat.OnValueChanged -= OnResourceValueChanged;
            _prodResourceStat = null;
            _statsCollection = null;
            
            _getResourceSystem.UnRegistration(ItemID, Id);
            _getResourceSystem.UnRegistration(GrabageItemId, Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        #endregion

        #region Production

        private void Production()
        {
            if (_finalized)
            {
                return;
            }

            var hasProduct = _inventory.HasItem(ItemID);
            var registredProduct = _getResourceSystem.Contains(ItemID, Id);
            
            if (hasProduct && !registredProduct) // Add Product
            {
                RegistrationProduction(ItemID, TakeAnim);
            }
            else if (!hasProduct && registredProduct) // Remove Product
            {
                _getResourceSystem.UnRegistration(ItemID, Id);
            }
            
            var hasGrabage = _inventory.HasItem(GrabageItemId);
            var registredGrabage = _getResourceSystem.Contains(GrabageItemId, Id);

            if (!hasProduct && hasGrabage && !registredGrabage) // Add Grabage
            {
                RegistrationProduction(GrabageItemId, TakeGrabageAnim);
            }
            else if ((hasProduct || !hasGrabage) && registredGrabage) // Remove Grabage
            {
                _getResourceSystem.UnRegistration(GrabageItemId, Id);
                if (!hasProduct && !hasGrabage)
                {
                    _instantiator.Instantiate<DeleteBuildingCommand>().Execute(new DeleteBuildingProtocol(Id));
                    return;
                }
            }

            UpdateStage();
        }

        private void RegistrationProduction(string itemId, AnimKey actionAnim)
        { 
            if (_finalized || _getResourceSystem.Contains(itemId, Id))
            {
                return;
            }

            var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
            var maxUnits = (int) _statsCollection.GetValue(_maxUnitsStatKey);
            var dto = _dtoStorage.Get(Id);
            var args = ResourceArgs.Default()
                .SetActionAnimKeys(actionAnim)
                .SetWalkAnimKeys(WalkAnim);
            var resourceProtocol = new GetResourceProtocol(Id, dto.ModelID, itemId, maxUnits,
                                                           taskPoints, TaskType, args);
            _getResourceSystem.Registration(resourceProtocol);
        }

        private void UpdateStage()
        {
            if(_finalized) return;
            
            var itemSlot = _inventory.GetItemSlot(ItemID);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        #endregion

        #region Resource Systems

        private void OnResourceChanged(string itemId, string guid)
        {
            if (Id != guid || _finalized)
            {
                return;
            }

            Production();
        }

        private void OnUpdateResourceInfo()
        {
            if (_finalized)
            {
                return;
            }
            
            var prependText = "";

            var itemSlot = _inventory.GetItemSlot(ItemID);
            if (itemSlot.Count == 0 && _inventory.HasItem(GrabageItemId))
            {
                prependText = LocalizationManager.Localize(_grabageTextKey) + " : ";
                itemSlot = _inventory.GetItemSlot(GrabageItemId);
            }

            _resourceInfo.SetInfo(prependText + Format.Resource(itemSlot.Count, itemSlot.Capacity));
        }
        
        private void OnResourceValueChanged(object sender, EventArgs e)
        {
            _inventory?.SetSlotCapacity(ItemID, (int)_statsCollection.GetValue(_prodResourceStatKey));
        }

    #endregion
    }
}