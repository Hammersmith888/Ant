using System;
using System.Linq;
using BugsFarm.BattleSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UniRx;

namespace BugsFarm.UI
{
    public class BattlePass
    {
        public event Action<bool> OnValidate;
        private readonly IActivitySystem _activitySystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly InventoryStorage _inventoryStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BattlePassModelsStorage _battlePassModelsStorage;
        private const string _fightStockModelId = "41";
        private const string _defaultItemId = "0";
        private IDisposable _stocksEvent;

        public BattlePass(IActivitySystem activitySystem,
                          UnitDtoStorage unitDtoStorage,
                          InventoryStorage inventoryStorage,
                          BuildingDtoStorage buildingDtoStorage,
                          StatsCollectionStorage statsCollectionStorage,
                          BattlePassModelsStorage battlePassModelsStorage)
        {
            _activitySystem = activitySystem;
            _battlePassModelsStorage = battlePassModelsStorage;
            _unitDtoStorage = unitDtoStorage;
            _inventoryStorage = inventoryStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _unitDtoStorage.OnStorageChanged += OnUnitsStorageChanged;
            _buildingDtoStorage.OnStorageChanged += OnBuildingStorageChanged;
            _activitySystem.OnStateChanged += OnActivityStateChanged;
            _stocksEvent = MessageBroker.Default.Receive<StockChangedProtocol>().Subscribe(OnStockChanged);
        }
        
        public void Validate()
        {
            OnValidate?.Invoke(ValidateFightStock() && ValidateUnits());
        }

        public void Dispose()
        {
            OnValidate = null;
            _stocksEvent?.Dispose();
            _stocksEvent = null;
            _unitDtoStorage.OnStorageChanged -= OnUnitsStorageChanged;
            _buildingDtoStorage.OnStorageChanged -= OnBuildingStorageChanged;
            _activitySystem.OnStateChanged -= OnActivityStateChanged;
        }

        private bool ValidateFightStock()
        {
            var fightStockDto = _buildingDtoStorage.Get().FirstOrDefault(x => x.ModelID == _fightStockModelId);
            if (fightStockDto == null || !_inventoryStorage.HasEntity(fightStockDto.Guid))
            {
                return false;
            }

            var inventory = _inventoryStorage.Get(fightStockDto.Guid);
            if (!inventory.HasItem(_defaultItemId))
            {
                return false;
            }

            var fighStockItemSlot = inventory.GetItemSlot(_defaultItemId);
            var minPassCost = _battlePassModelsStorage.Get().Min(x => x.Params["cost"].MaxValue);
            return !(fighStockItemSlot.Count < minPassCost);
        }

        private bool ValidateUnits()
        {
            if (!_unitDtoStorage.Any())
            {
                return false;
            }
            
            foreach (var passModel in _battlePassModelsStorage.Get())
            {
                var unitDtos = _unitDtoStorage.Get()
                    .Where(x => x.ModelID == passModel.ModelID)
                    .ToArray();
                if (unitDtos.Length == 0)
                {
                    continue;
                }

                if (unitDtos.Any(unitDto => passModel.Params.Count == 1 || 
                    passModel.Params.All(param => ValidateParam(unitDto.Guid, param.Key))))
                {
                    return true;
                }
            }
            return false;
        }

        private bool ValidateParam(string unitId, string paramId)
        {
            if (paramId == "cost")
            {
                return true;
            }
            
            if (!_unitDtoStorage.HasEntity(unitId))
            {
                return false;
            }
            
            var statKey = "stat_" + paramId;
            var unitDto = _unitDtoStorage.Get(unitId);
            var passModel = _battlePassModelsStorage.Get(unitDto.ModelID);
            var param = passModel.Params[paramId];
            var statCollection = _statsCollectionStorage.Get(unitDto.Guid);
            var stat = statCollection.Get<StatVital>(statKey);
            if (param.MinValue != 0)
            {
                return stat.CurrentValue >= param.MinValue && stat.CurrentValue <= param.MaxValue;
            }
            return stat.CurrentValue >= param.MaxValue;
        }

        private void OnStockChanged(StockChangedProtocol protocol)
        {
            if (string.IsNullOrEmpty(protocol.ModelId) || protocol.ModelId != _fightStockModelId)
            {
                return;
            }
            Validate();
        }
        
        private void OnBuildingStorageChanged(string buildingId)
        {
            if (!_buildingDtoStorage.HasEntity(buildingId) || 
                _buildingDtoStorage.Get(buildingId).ModelID != _fightStockModelId)
            {
                return;
            }
            
            Validate();
        }

        private void OnUnitsStorageChanged(string unitId)
        {
            Validate();
        }
        
        private void OnActivityStateChanged(string entityId)
        {
            Validate();
        }
    }
}