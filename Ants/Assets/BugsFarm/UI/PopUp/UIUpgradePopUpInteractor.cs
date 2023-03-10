using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class UIUpgradePopUpInteractor
    {
        private readonly BuildingShopItemModelStorage _shopItemModelStorage;
        private readonly IInputController<MainLayer> _inputController;
        private readonly UnitShopItemsStorage _unitShopItemsStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly IInstantiator _instantiator;
        private readonly IconLoader _iconLoader;
        private readonly IUIService _uiService;
        private readonly IUser _user;

        private List<UIUpgradePopUpUnlockableItem> _itemsList;
        private UIUpgradePopUpWindow _popup;

        public UIUpgradePopUpInteractor(IUIService uiService,
            IInstantiator instantiator,
            IInputController<MainLayer> inputController,
            BuildingShopItemModelStorage shopItemModelStorage,
            UnitShopItemsStorage unitShopItemsStorage,
            IconLoader iconLoader,
            BuildingModelStorage buildingModelStorage,
            UnitModelStorage unitModelStorage,
            IUser user)
        {
            _instantiator = instantiator;
            _iconLoader = iconLoader;
            _inputController = inputController;
            _buildingModelStorage = buildingModelStorage;
            _unitModelStorage = unitModelStorage;
            _shopItemModelStorage = shopItemModelStorage;
            _unitShopItemsStorage = unitShopItemsStorage;
            _itemsList = new List<UIUpgradePopUpUnlockableItem>();
            _user = user;
            _uiService = uiService;
        }

        public void OpenPopUp()
        {
            _popup = _uiService.Show<UIUpgradePopUpWindow>();
            _popup.SetLevel(_user.GetLevel().ToString());
            _popup.CloseEvent += CloseWindow;
            _inputController.Lock();
            var userLevel = _user.GetLevel();

            SpawnBuildingItems(userLevel);
            SpawnUnitItems(userLevel);
            
            if(_itemsList.Count == 0)
                _popup.SwitchToShortSize();
            else
                _popup.SwitchToFullSize();
        }

        private void SpawnBuildingItems(int userLevel)
        {
            var openedBuildingItems = _shopItemModelStorage.Get().Where(x => x.Levels.First().Value.Level == userLevel);

            foreach (var openedBuildingItem in openedBuildingItems)
            {
                var slot = _instantiator.InstantiatePrefabForComponent<UIUpgradePopUpUnlockableItem>(_popup.Prefab, _popup.ParentTransform);
                var name = LocalizationHelper.GetBuildingName(openedBuildingItem.ModelId);
                slot.SetItemTitle(name);
                slot.SetIcon(_iconLoader.Load(_buildingModelStorage.Get(openedBuildingItem.ModelId).TypeName));
                _itemsList.Add(slot);
            }
        }

        private void SpawnUnitItems(int userLevel)
        {
            var openedUnitItems = _unitShopItemsStorage.Get().Where(x => x.BuyLevel == userLevel);

            foreach (var openedUnitItem in openedUnitItems)
            {
                var slot = _instantiator.InstantiatePrefabForComponent<UIUpgradePopUpUnlockableItem>(_popup.Prefab, _popup.ParentTransform);
                var name = LocalizationHelper.GetBugTypeName(openedUnitItem.ModelID);
                slot.SetItemTitle(name);
                slot.SetIcon(_iconLoader.Load(_unitModelStorage.Get(openedUnitItem.ModelID).TypeName));
                _itemsList.Add(slot);
            }
        }
        
        private void CloseWindow(object sender, EventArgs e)
        {
            var popup = _uiService.Get<UIUpgradePopUpWindow>();
            _inputController.UnLock();
            popup.HidedEvent += DisposeItems;
            _uiService.Hide<UIUpgradePopUpWindow>();
        }

        private void DisposeItems()
        {
            foreach (var item in _itemsList)
            {
                GameObject.Destroy(item.gameObject);
            }
            _itemsList.Clear();

        }
    }
}