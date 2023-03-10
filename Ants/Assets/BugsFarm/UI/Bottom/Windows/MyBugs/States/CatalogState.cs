using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.UI
{
    public class CatalogState : State
    {
        private readonly IUser _user;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly MyBugsItemModelStorage _myBugsItemModelStorage;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitShopItemsStorage _unitShopItemsStorage;
        private readonly UnitStatModelStorage _unitStatModelStorage;
        private readonly IconLoader _iconLoader;
        private readonly List<CatalogItem> _itemCells;
        private readonly CompositeDisposable _events;
        private const string _availableFormTextKey = "UIMyBugs_CatalogAvailableFrom";
        private const string _maxDisplayValueStatKey = "stat_maxDisplayValue";
        private const string _levelTextKet = "LVL";


        public CatalogState(IInstantiator instantiator,
                            IUIService uiService,
                            IUser user,
                            StatsCollectionStorage statsCollectionStorage,
                            MyBugsItemModelStorage myBugsItemModelStorage,
                            UnitModelStorage unitModelStorage,
                            UnitShopItemsStorage unitShopItemsStorage,
                            UnitStatModelStorage unitStatModelStorage,
                            IconLoader iconLoader) : base("Catalog")
        {
            _user = user;
            _statsCollectionStorage = statsCollectionStorage;
            _myBugsItemModelStorage = myBugsItemModelStorage;
            _uiService = uiService;
            _instantiator = instantiator;
            _unitModelStorage = unitModelStorage;
            _unitShopItemsStorage = unitShopItemsStorage;
            _unitStatModelStorage = unitStatModelStorage;
            _iconLoader = iconLoader;
            _events = new CompositeDisposable();
            _itemCells = new List<CatalogItem>();
        }

        public override void OnEnter(params object[] args)
        {
            MessageBroker.Default
                .Receive<UserCurrencyChangedProtocol>()
                .Subscribe(OnUserCurrencyChangedEventHandler)
                .AddTo(_events);

            var view = _uiService.Get<UIMyBugsWindow>().CatalogView;
            view.Show();
            var prefabItem = view.PrefabItem;
            var container = view.ItemsContainer;
            var shopItemModels = _unitShopItemsStorage.Get().ToList();
            var sortedModelItems = shopItemModels.OrderBy(x => x.BuyLevel);

            foreach (var shopItemModel in sortedModelItems)
            {
                var model = _unitModelStorage.Get(shopItemModel.ModelID);
                var shopItem = _instantiator.InstantiatePrefabForComponent<CatalogItem>(prefabItem, container);
                var itemIcon = _iconLoader.Load(model.TypeName);
                var itemHeader = LocalizationHelper.GetBugTypeName(shopItemModel.ModelID);
                
                // set view
                shopItem.SetIcon(itemIcon);
                shopItem.SetHeader(itemHeader);
                
                var availableLevel = _user.GetLevel() >= shopItemModel.BuyLevel;
                var hasCurrency = HasCurrency(shopItemModel.ModelID);
           
                shopItem.Id = shopItemModel.ModelID;
                shopItem.SetCanBuy(availableLevel);
                shopItem.SetInteractable(availableLevel && hasCurrency);
                shopItem.SetPriceIcon(availableLevel ?_iconLoader.LoadCurrency(shopItemModel.Price.ModelID) : null);
                shopItem.SetPrice(shopItemModel.Price.Count.ToString());
                var availableFromText = LocalizationManager.Localize(_availableFormTextKey);
                var levelText = LocalizationManager.Localize(_levelTextKet);
                shopItem.SetLowLevelText(string.Format(availableFromText, shopItemModel.BuyLevel, levelText));
                
                if(_myBugsItemModelStorage.TryGet(shopItemModel.ModelID, out var myBugItemModel) &&
                   _unitStatModelStorage.TryGet(shopItemModel.ModelID, out var statsModel))
                {
                    var maxDisplayStatValue = _statsCollectionStorage.Get(_user.Id).GetValue(_maxDisplayValueStatKey);
                    foreach (var statModel in statsModel.Stats)
                    {
                        if (myBugItemModel.Stats.Contains(statModel.StatID))
                        {
                            var paramItem = _instantiator
                                .InstantiatePrefabForComponent<ParamItem>(shopItem.ParamItemPrefab, shopItem.ParamsContainer);
                            paramItem.SetProgress(Mathf.Clamp01(statModel.BaseValue / maxDisplayStatValue));
                            paramItem.SetProgressText(""); // TODO : fill if value overflow of max display value
                            paramItem.SetIcon(_iconLoader.Load(statModel.StatID.Replace("stat_",""))); 
                        }
                    }
                }

                
                // events
                shopItem.BuyClickEvent += (sender, id) =>
                {
                    if (HasCurrency(id))
                    {

                        _user.AddCurrency(shopItemModel.Price.ModelID, -shopItemModel.Price.Count);
                        var unitBuildingProtocol = new CreateUnitProtocol(id, true);
                        _instantiator.Instantiate<CreateUnitCommand>().Execute(unitBuildingProtocol);

                        var spawnProtocol = new UnitSpawnProtocol(unitBuildingProtocol.Guid);
                        _instantiator.Instantiate<UnitSpawnCommand<SpawnUnitTask>>().Execute(spawnProtocol);
                        
                        MessageBroker.Default.Publish(new AntHillTaskActionCompletedProtocol()
                        {
                            Guid = spawnProtocol.Guid,
                            EntityType = AntHillTaskReferenceGroup.Unit,
                            TaskType = AntHillTaskType.Build,
                            ModelID = id
                        });
                        
                        return;
                    }

                    shopItem.SetInteractable(false);
                };
                shopItem.InfoEvent += (sender, id) =>
                {
                    var hintWindow = _uiService.Show<UIHintWindow>();
                    hintWindow.SetIcon(itemIcon);
                    hintWindow.SetHint(LocalizationHelper.GetBugDescription(shopItemModel.ModelID));
                    hintWindow.SetHeader(itemHeader);
                    hintWindow.CloseEvent += (o, eventArgs) =>
                    {
                        _uiService.Hide<UIHintWindow>();
                    };
                };
                _itemCells.Add(shopItem);
            }
        }

        public override void OnExit()
        {
            base.OnExit();
            _events?.Dispose();
            _events?.Clear();
            _uiService.Get<UIMyBugsWindow>().CatalogView.Hide();
            foreach (var itemCell in _itemCells)
            {
                itemCell.Clear();
                Object.Destroy(itemCell.gameObject);
            }

            _itemCells.Clear();
        }

        private bool HasCurrency(string modelId)
        {
            if (!_unitShopItemsStorage.HasEntity(modelId))
            {
                return false;
            }

            var shopItemModel = _unitShopItemsStorage.Get(modelId);
            return _user.HasCurrency(shopItemModel.Price);
        }

        private void OnUserCurrencyChangedEventHandler(UserCurrencyChangedProtocol protocol)
        {
            foreach (var itemCell in _itemCells)
            {
                itemCell.SetInteractable(HasCurrency(itemCell.Id));
            }
        }
    }
}