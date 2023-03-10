using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.UI
{
    public class FarmShopInteractor
    {
        public event Action<string> OnCellBuyClicked;
        public event Action<string> OnCellClicked;
        public event Action<int> OnPageSwitch;
        
        private readonly BuildingShopItemModelStorage _buildingShopItemModelStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;
        private readonly IUser _user;
        private readonly IconLoader _iconLoader;
        private readonly List<UIFarmShopCellView> _cells;
        private const string _availableFormTextKey = "UIFarmShop_AvailableFrom";
        private const string _levelTextKet = "LVL";
        private Dictionary<int, List<string>> _shopModels;
        private bool _initialized;
        private UIFarmShopWindow _window;

        public FarmShopInteractor(BuildingShopItemModelStorage buildingShopItemModelStorage,
                                  BuildingModelStorage buildingModelStorage,
                                  BuildingDtoStorage buildingDtoStorage,
                                  IInstantiator instantiator,
                                  IconLoader iconLoader,
                                  IUIService uiService,
                                  IUser user)
        {
            _buildingModelStorage = buildingModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _instantiator = instantiator;
            _buildingShopItemModelStorage = buildingShopItemModelStorage;
            _uiService = uiService;
            _user = user;
            _iconLoader = iconLoader;
            _cells = new List<UIFarmShopCellView>();
        }

        public void Init(int tabIndex = 0)
        {
            if (_initialized) return;
            _initialized = true;
            _window = _uiService.Show<UIFarmShopWindow>();
            _shopModels = new Dictionary<int, List<string>>();
            foreach (var shopItemModel in _buildingShopItemModelStorage.Get())
            {
                if (!_shopModels.ContainsKey(shopItemModel.TypeId))
                {
                    _shopModels.Add(shopItemModel.TypeId, new List<string>());
                }

                _shopModels[shopItemModel.TypeId].Add(shopItemModel.ModelId);
            }

            foreach (var tabElement in _window.ShopTabs)
            {
                tabElement.ClickEvent += (sender, index) => SelectTabElement(index);
            }

            SelectTabElement(tabIndex);
        }

        public void Dispose()
        {
            if (!_initialized) return;
            _uiService.Hide<UIFarmShopWindow>();
            OnPageSwitch = null;
            OnCellBuyClicked = null;
            OnCellClicked = null;
            _initialized = false;
            _shopModels.Clear();
            Clear();
        }

        public void SelectTabElement(int tabIndex)
        {
            if (!_initialized) return;
            var tabs = _window.ShopTabs;
            for (var i = 0; i < tabs.Length; i++)
            {
                if (i == tabIndex)
                {
                    tabs[i].Show();
                }
                else
                {
                    tabs[i].Hide();
                }
            }

            OnPageSwitch?.Invoke(tabIndex);
            FillContent(tabIndex);
        }

        public void BuySuccess(string modelId)
        {
            if (!_buildingShopItemModelStorage.HasEntity(modelId))
            {
                throw new ArgumentException($"Building with modelId : {modelId}, does not exist");
            }
            var itemModel = _buildingShopItemModelStorage.Get(modelId);
            _user.AddCurrency(itemModel.Price.ModelID, -itemModel.Price.Count);
        }

        private void FillContent(int typeId)
        {
            if (!_initialized) return;
            Clear();
            foreach (var itemModelId in _shopModels[typeId])
            {
                CreateItem(itemModelId);
            }

            var sorted = _cells.OrderBy(x => x.SortCost).ToArray();
            for (var i = 0; i < sorted.Length; i++)
            {
                var cellView = sorted[i];
                cellView.transform.SetSiblingIndex(i);
            }
        }

        private void CreateItem(string modelId)
        {
            if (!_initialized) return;

            var model = _buildingModelStorage.Get(modelId);
            var itemModel = _buildingShopItemModelStorage.Get(modelId);
            var userLevel = _user.GetLevel();
            
            var prefab = _window.CellViewPrefab;
            var content = _window.Content;
            var cellView = _instantiator.InstantiatePrefabForComponent<UIFarmShopCellView>(prefab, content);
            
            var nextLevel = 0;
            var itemLevel = int.MaxValue;
            var maxCount = 0;
            foreach (var itemLevelModel in itemModel.Levels.Values.OrderBy(x => x.Level))
            {
                // find all counts under user level
                if (itemLevelModel.Level <= userLevel)
                {
                    maxCount += itemLevelModel.Count;
                    itemLevel = itemLevelModel.Level;
                }
                // find next level above user level
                if (nextLevel == 0 && itemLevelModel.Level > userLevel)
                {
                    nextLevel = itemLevelModel.Level;
                }
            }
            
            var currentCount = _buildingDtoStorage.Get().Count(x=>x.ModelID == modelId);
            var availableItem = (maxCount - currentCount > 0 || maxCount == 0) && itemLevel <= userLevel;
            var color = !availableItem ? "red" : "";
            
            cellView.Init(modelId);
            cellView.OnInfoClicked += OnCellClicked;
            cellView.OnByClicked += OnCellBuyClicked;

            cellView.SetCountText(Format.ResourceColored(currentCount, color, maxCount, color , true));
            cellView.SetCountActive(maxCount > 0);

            var hasCurrency = _user.HasCurrency(itemModel.Price);
            var canBuy = availableItem && hasCurrency;
            var alpha = canBuy ? 1 : 0.5f;
            var priceCount = itemModel.Price.Count;
            cellView.SetBuyCostText(priceCount.ToString());
            cellView.SetBuyButtonActive(availableItem || nextLevel == 0);
            cellView.SetBuyInteractable(canBuy);
            cellView.SetBuyAlpha(alpha);

            var lockText = "";
            var lockActive = !availableItem && nextLevel > 0;
            if (lockActive)
            {
                var text = LocalizationManager.Localize(_availableFormTextKey);
                var level = LocalizationManager.Localize(_levelTextKet);
                lockText = string.Format(text, nextLevel, level);
            }
            cellView.SetLockActive(lockActive);
            cellView.SetLokedText(lockText);
            
            cellView.SetInfoIcon(_iconLoader.LoadOrDefault(model.TypeName, itemModel.TypeId));
            cellView.SetInfoInteractable(canBuy);
            cellView.SetInfoAlpha(alpha);
            
            if (availableItem) // если доступно
            {
                if (hasCurrency) // если есть денежки
                {
                    cellView.SortCost = priceCount; // sort layer
                }
                else // если нет денежек
                {
                    cellView.SortCost = priceCount + 1000000; // sort layer
                }
            }
            else if(nextLevel == 0) // если не доступно по количеству
            {
                cellView.SortCost = priceCount + 2000000; // sort layer
            }
            else // если не доступно по уровню
            {
                cellView.SortCost = nextLevel + 3000000; // sort layer
            }
            
            _cells.Add(cellView);
        }

        private void Clear()
        {
            foreach (var cellView in _cells)
            {
                cellView.OnInfoClicked -= OnCellClicked;
                cellView.OnByClicked -= OnCellBuyClicked;
                UnityEngine.Object.Destroy(cellView.gameObject);
            }

            _cells.Clear();
        }
    }
}