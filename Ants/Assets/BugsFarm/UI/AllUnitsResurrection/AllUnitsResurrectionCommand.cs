using System;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;

namespace BugsFarm.UI
{
    public class AllUnitsResurrectionCommand
    {
        private readonly IUIService _uiService;
        private readonly IInputController<MainLayer> _inputController;
        private readonly UnitCivilRegistrySystem _civilRegistrySystem;
        private readonly ResurrectBuildingRegistrySystem _resurrectBuildingRegistrySystem;
        private readonly UnitShopItemsStorage _unitShopItemsStorage;
        private readonly UnitUpgradeModelStorage _upgradeModelStorage;
        private readonly UnitCivilRegistryDtoStorage _unitCivilRegistryDtoStorage;
        private readonly IUser _user;

        private const string _descriptionTextKey = "UIAllUnitsResurrection_Discription";
        private const string _startOverButtonTextKey = "UIAllUnitsResurrection_StartOverButton";
        private const string _resurrectButtonTextKey = "UIAllUnitsResurrection_ResurrectButton";
        private const string _diamondsModelId = "1";
        private const int _diamondsCoefficient = 25;

        private const string _farmShopBottomWindow = "FarmShop";
        private const string _myBugsBottomWindow = "MyBugs";
        
        private UIAllUnitsResurrectionWindow _window;

        private int _resurrectionPrice;

        public AllUnitsResurrectionCommand(IUIService uiService, 
                                           IInputController<MainLayer> inputController,
                                           UnitCivilRegistrySystem civilRegistrySystem,
                                           ResurrectBuildingRegistrySystem resurrectBuildingRegistrySystem,
                                           UnitShopItemsStorage unitShopItemsStorage,
                                           UnitUpgradeModelStorage upgradeModelStorage,
                                           UnitCivilRegistryDtoStorage unitCivilRegistryDtoStorage,
                                           IUser user)
        {
            _user = user;
            _unitCivilRegistryDtoStorage = unitCivilRegistryDtoStorage;
            _upgradeModelStorage = upgradeModelStorage;
            _unitShopItemsStorage = unitShopItemsStorage;
            _resurrectBuildingRegistrySystem = resurrectBuildingRegistrySystem;
            _civilRegistrySystem = civilRegistrySystem;
            _inputController = inputController;
            _uiService = uiService;
        }
        public void Execute()
        {
            _inputController.Lock();
            Setup();
            ConfigurePrice();
        }

        private void ConfigurePrice()
        {
            int totalPriceInGolds = 0;

            foreach (var civilRegistry in _unitCivilRegistryDtoStorage.Get())
            {
                string modelID = civilRegistry.MoverDto.ModelID;
                totalPriceInGolds += GetDefaultPrice(modelID);
                totalPriceInGolds += GetUpgradePrice(modelID, civilRegistry.UnitLevel);
            }

            int totalPriceInDiamonds = ConvertToDiamonds(totalPriceInGolds);

            _resurrectionPrice = totalPriceInDiamonds;
            _window.SetPrice(totalPriceInDiamonds.ToString());
        }

        private int ConvertToDiamonds(int totalPriceInGolds)
        {
            return totalPriceInGolds / _diamondsCoefficient;
        }

        private int GetUpgradePrice(string modelID, int unitLevel)
        {
            if (!_upgradeModelStorage.HasEntity(modelID))
                return 0;

            int upgradePrice = 0;
            
            UnitUpgradeModel upgradeModel = _upgradeModelStorage.Get(modelID);
            
            for (int i = 2; i < unitLevel + 2; i++)
            {
                if (i >= upgradeModel.Levels.Count)
                {
                    break;
                }
                
                var level = upgradeModel.Levels[i];
                upgradePrice += level.Price.Count;
            }

            return upgradePrice;
        }

        private int GetDefaultPrice(string modelID)
        {
            if (_unitShopItemsStorage.HasEntity(modelID))
                return _unitShopItemsStorage.Get(modelID).Price.Count;
            return 0;
        }

        private void Setup()
        {
            _window = _uiService.Show<UIAllUnitsResurrectionWindow>();
            _window.StartOverEvent += StartOverGame;
            _window.ResurrectEvent += CheckUsersCurrency;
            SetBottomStatesInteractable(false);
            SetInfoToWindow();
        }

        private void SetInfoToWindow()
        {
            _window.SetDescription(LocalizationManager.Localize(_descriptionTextKey));
            _window.SetTextToStartOverButton(LocalizationManager.Localize(_startOverButtonTextKey));
            _window.SetTextToResurrectButton(LocalizationManager.Localize(_resurrectButtonTextKey));
        }

        private void CheckUsersCurrency(object sender, EventArgs e)
        {
            if(_user.HasCurrency(new CurrencyModel(){ModelID = _diamondsModelId, Count = _resurrectionPrice}))
            {
                ResurrectsUnits();
            }
            else
            {
                OpenDonateWindow();
            }
        }

        private void OpenDonateWindow()
        {
            _window.OpenDonateWindow();
        }

        private void ResurrectsUnits()
        {
            _user.AddCurrency(_diamondsModelId, -_resurrectionPrice);
            _civilRegistrySystem.ResurrectAllUnits();
            _resurrectBuildingRegistrySystem.ResurrectAllBuildings();
            _uiService.Hide<UIAllUnitsResurrectionWindow>();
            SetBottomStatesInteractable(true);
            _inputController.UnLock();
        }

        private void SetBottomStatesInteractable(bool isInteractable)
        {
            var bottomWindow = _uiService.Get<UIBottomWindow>();
            bottomWindow.SetInteractable(_farmShopBottomWindow, isInteractable);
            bottomWindow.SetInteractable(_myBugsBottomWindow, isInteractable);
        }
        private void StartOverGame(object sender, EventArgs e)
        {
        }
    }
}