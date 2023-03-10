using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class BuildingInfoMenuRestock : InteractionBaseCommand
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingUpgradeModelStorage _upgradeModelStorage;
        private readonly BuildingRestockModelStorage _buildingRestockModelStorage;
        private readonly BuildingParamsModelStorage _paramModelStorage;
        private readonly InventoryStorage _inventoryStorage;
        private const string _restockButtonLableTextKey = "UIBuildingInfoMenu_RestockLabel";
        private BuildingRestockModel _restockModel;
        private BuildingDto _buildingDto;
        private IInventory _inventory;
        
        public BuildingInfoMenuRestock(IUser user,
                                       IUIService uiService,
                                       IInstantiator instantiator,
                                       BuildingDtoStorage buildingDtoStorage,
                                       BuildingUpgradeModelStorage upgradeModelStorage,
                                       BuildingRestockModelStorage buildingRestockModelStorage,
                                       BuildingParamsModelStorage paramModelStorage,
                                       InventoryStorage inventoryStorage)
        {
            _user = user;
            _uiService = uiService;
            _instantiator = instantiator;
            _buildingDtoStorage = buildingDtoStorage;
            _upgradeModelStorage = upgradeModelStorage;
            _buildingRestockModelStorage = buildingRestockModelStorage;
            _paramModelStorage = paramModelStorage;
            _inventoryStorage = inventoryStorage;
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            if (!_buildingDtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"Building with Id : {protocol.Guid}" +
                                            $", does not exist");
            }
            
            _buildingDto = _buildingDtoStorage.Get(protocol.Guid);
            var buildingId = protocol.Guid;
            var modelId = _buildingDto.ModelID;
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            if (!CanRestock(modelId))
            {
                window.SetRestockButton(false);
                _buildingDto = null;
                return base.Execute(protocol);
            }
            
            if (!_buildingRestockModelStorage.HasEntity(modelId))
            {
                throw new InvalidOperationException($"Building with ModelID : {modelId}" +
                                                    $", does not have {nameof(BuildingRestockModel)}!");
            }
            
            if (!_inventoryStorage.HasEntity(buildingId))
            {
                throw new InvalidOperationException($"Building with ModelID : {modelId}" +
                                                    $", does not have {nameof(IInventory)}!");
            }
            
            _inventory = _inventoryStorage.Get(buildingId);
            _restockModel = _buildingRestockModelStorage.Get(modelId);
            _inventory.UpdateInventoryAction += OnUpdateRestockHadler;
            
            window.CloseEvent += (sender, e) =>
            {
                if (_inventory != null)
                {
                    _inventory.UpdateInventoryAction -= OnUpdateRestockHadler;
                }

                _buildingDto = null;
                _inventory = null;
            };
            
            window.RestockEvent += (sender, e) =>
            {
                if (GetRestockCost(out var price, out var count) && _user.HasCurrency(price))
                {

                    _instantiator.Instantiate<RestockBuildingCommand>()
                        .Execute(new RestockBuildingProtocol(buildingId, _restockModel.ItemId, count));
                    _user.AddCurrency(price.ModelID, -price.Count);
                    window.Close();
                }
                else
                {
                    OnUpdateRestockHadler(_restockModel.ItemId);
                }
            };

            OnUpdateRestockHadler(_restockModel.ItemId);
            return base.Execute(protocol);
        }

        private bool CanRestock(string modelId)
        {
            return _paramModelStorage.HasEntity(modelId) &&
                   _paramModelStorage.Get(modelId).Params.Contains("Restock");
        }
        
        private bool GetRestockCost(out CurrencyModel price, out int count)
        {
            price = default;
            count = 0;
            if (_inventory == null || _buildingDto == null)
            {
                return false;
            }

            if (!_inventory.HasItemSlot(_restockModel.ItemId))
            {
                return false;
            }
            
            var itemSlot = _inventory.GetItemSlot(_restockModel.ItemId);
            if (itemSlot.Capacity <= 0)
            {
                throw new InvalidOperationException($"Building with ModelID : {_buildingDto.ModelID}" +
                                                    $", ItemID : {_restockModel.ItemId}"  +
                                                    $", does not have limit of Capacity : {_buildingDto.ModelID}");
            }

            price = _restockModel.Price;
            count = itemSlot.Capacity - itemSlot.Count;
            var percent01 = ((float) itemSlot.Count / itemSlot.Capacity);
            var percent = percent01 * 100f;
            var canRestock = percent >= _restockModel.MinPrecent &&
                             percent <= _restockModel.MaxPrecent;
            var upgradeCost = 0;
            if (canRestock && _upgradeModelStorage.HasEntity(_buildingDto.ModelID))
            {
                var upgradeModel = _upgradeModelStorage.Get(_buildingDto.ModelID);
                _instantiator.Instantiate<GetEntityLevelCommand>()
                    .Execute(new GetEntitytLevelProtocol(_buildingDto.Guid, level =>
                    {
                        if (level > 0)
                        {
                            upgradeCost = upgradeModel.Levels
                                .Where(upgradeLevel => upgradeLevel.Key <= level)
                                .Sum(x => x.Value.Price.Count);
                        }
                    }));

            }

            price.Count = Mathf.RoundToInt((1f - percent01) * (price.Count + upgradeCost));
            return canRestock;
        }
        
        private void OnUpdateRestockHadler(string itemId)
        {
            if (string.IsNullOrEmpty(itemId) ||
                _restockModel.ItemId != itemId || 
                _buildingDto == null)
            {
                return;
            }

            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            window.SetRestockButton(GetRestockCost(out var price, out _));
            window.SetRestockButtonInterractable(_user.HasCurrency(price));
            window.SetRestockButtonLabel(LocalizationManager.Localize(_restockButtonLableTextKey));
            window.SetRestockCost(price.Count.ToString());
        }
    }
}