using System;
using System.Globalization;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class MudStock : ISceneEntity, IInitializable, IDisposableHandler
    {
        public string Id { get; private set; }
        private string ItemID => "2"; // предмет земли
        private AnimKey AddActionAnimKey => AnimKey.DropMud;
        private AnimKey AddWalkAnimKey => AnimKey.WalkMud;
        private Type TaskType => typeof(AddMudStockTask);

        private AddResourceSystem _addResourceSystem;
        private StatsCollectionStorage _statsCollectionStorage;
        private ICurrencyCollectorSystem _currencyCollection;
        private BuildingSceneObjectStorage _viewStorage;
        private BuildingDtoStorage _buildingDtoStorage;
        private IReservedPlaceSystem _reservedPlaceSystem;
        private PlaceIdStorage _placeIdStorage;
        private IInstantiator _instantiator;
        
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _maxResourceStatKey = "stat_maxResource";
        private const string _rewardCountStatKey = "stat_rewardCount";
        private const string _preProductionStatKey = "stat_preProduction";
        private const string _rewardIdStatKey = "stat_rewardCurrencyID";
        private const string _rewardPercentStatKey = "stat_rewardAfterPercent";

        private bool _finalized;
        private IInventory _inventory;
        private BuildingSceneObject _view;
        private ResourceInfo _resourceInfo;
        private StatsCollection _statsCollection;
        private StatModifiable _preProductionStat;
        private StatVital _resourceStat;
        private IUser _user;

        [Inject]
        private void Inject(string guid,
                            IUser user,
                            AddResourceSystem addResourceSystem,
                            ICurrencyCollectorSystem currencyCollection,
                            StatsCollectionStorage statCollectionStorage,
                            BuildingSceneObjectStorage viewSotrage,
                            BuildingDtoStorage dtoStorage,
                            IInstantiator instantiator,
                            IReservedPlaceSystem reservedPlaceSystem,
                            PlaceIdStorage placeIdStorage)
        {
            Id = guid;
            _user = user;
            _addResourceSystem = addResourceSystem;
            _statsCollectionStorage = statCollectionStorage;
            _instantiator = instantiator;
            _currencyCollection = currencyCollection;
            _buildingDtoStorage = dtoStorage;
            _viewStorage = viewSotrage;
            _reservedPlaceSystem = reservedPlaceSystem;
            _placeIdStorage = placeIdStorage;
        }

        public void Initialize()
        {
            _view = _viewStorage.Get(Id);
            _statsCollection = _statsCollectionStorage.Get(Id);
            _resourceStat = _statsCollection.Get<StatVital>(_maxResourceStatKey);
            _preProductionStat = _statsCollection.Get<StatModifiable>(_preProductionStatKey);
            _preProductionStat.OnValueChanged += OnPreProductionChanged;
            
            var slot = new ItemSlot(ItemID, 0, (int) _resourceStat.Value);
            var inventoryProtocol = new CreateInventoryProtocol(Id, res=>_inventory = res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);
            
            _addResourceSystem.OnResourceFull += OnResourceChanged;
            _addResourceSystem.OnResourceChanged += OnResourceChanged;
            
            _resourceInfo = _instantiator.Instantiate<ResourceInfo>(new object[]{Id});
            _resourceInfo.OnUpdate += OnUpdateResourceInfo;

            UpdateStage();
            Production();
        }

        public void Dispose()
        {
            if (_finalized)return;

            _finalized = true;
            _resourceInfo.OnUpdate -= OnUpdateResourceInfo;
            _resourceInfo.Dispose();
            _resourceInfo = null;
            
            _preProductionStat.OnValueChanged -= OnPreProductionChanged;
            _statsCollection = null;
            _preProductionStat = null;
            _resourceStat = null;
            
            _addResourceSystem.OnResourceFull -= OnResourceChanged;
            _addResourceSystem.OnResourceChanged -= OnResourceChanged;
            _addResourceSystem.UnRegistration(ItemID, Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        public void OnDisposeHandle()
        {
            if (_finalized || _preProductionStat.Value == 0)return;

            var itemSlot = _inventory.GetItemSlot(ItemID);
            var currPercent = ((float)itemSlot.Count / itemSlot.Capacity) * 100f;
            
            if (currPercent >= _statsCollection.GetValue(_rewardPercentStatKey))
            {
                var rewardCount = (int)_statsCollection.GetValue(_rewardCountStatKey);
                var currencyId = _statsCollection.GetValue(_rewardIdStatKey).ToString(CultureInfo.InvariantCulture);
                _currencyCollection.Collect(_view.transform.position,
                                            currencyId,
                                            rewardCount,
                                            true,
                                            left =>
                                            {
                                                var rewarded = rewardCount - left;
                                                rewardCount = left;
                                                _user.AddCurrency(currencyId, rewarded);
                                            });
            }
        }

        private void Production()
        {
            if (_finalized || _preProductionStat.Value == 0)
            {
                return;
            }

            var itemSlot = _inventory.GetItemSlot(ItemID);
            var needAdd = itemSlot.Count < itemSlot.Capacity;
            var registred = _addResourceSystem.Contains(ItemID, Id);            
            var dto = _buildingDtoStorage.Get(Id);
            var hasPlace = HasPlaceNum(dto.ModelID);

            if (hasPlace && needAdd && !registred)
            {
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
                var args = ResourceArgs.Default()
                    .SetActionAnimKeys(AddActionAnimKey)
                    .SetWalkAnimKeys(AddWalkAnimKey);
                var protocol = new AddResourceProtocol(Id, dto.ModelID, ItemID, maxUnits, taskPoints,
                                                       TaskType, args, false);
                _addResourceSystem.Registration(protocol);
            }
            else if ((dto.PlaceNum == PlaceBuildingUtils.OffScreenPlaceNum && !hasPlace && registred) || (!needAdd && registred))
            {
                _addResourceSystem.UnRegistration(ItemID, Id);
            }
        }

        private bool HasPlaceNum(string modelID)
        {
            return _placeIdStorage.Get().Any(x=> x.HasPlace(modelID) && !_reservedPlaceSystem.HasEntity(x.PlaceNumber));
        }

        private void UpdateStage()
        {
            if (_finalized) return;

            var itemSlot = _inventory.GetItemSlot(ItemID);
            var protocol = new SetStageBuildingProtocol(Id, itemSlot.Count, itemSlot.Capacity, OnSetStage);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
        }

        private void OnSetStage(StageActionProtocol protocol)
        {
            _view.SetInterractable(protocol.CurIndex != protocol.MaxIndex);
        }

        private void OnResourceChanged(string itemId, string guid)
        {
            if (guid != Id || itemId != ItemID || _finalized)
            {
                return;
            }

            UpdateStage();
            Production();
        }

        private void OnPreProductionChanged(object sender, EventArgs e)
        {
            if (_finalized)
            {
                return;
            }
            _inventory.SetDefaultCapacity((int)_resourceStat.Value);
            UpdateStage();
            Production();
        }
        
        private void OnUpdateResourceInfo()
        {
            if (_finalized) return;

            var slot = _inventory.GetItemSlot(ItemID);
            _resourceInfo.SetInfo(Format.Resource(slot.Count, slot.Capacity));
        }
    }
}