using System.Collections.Generic;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class DonatShopState : State
    {
        private readonly IStateMachine _stateMachine;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly UIRoot _uiRoot;
        private readonly IconLoader _iconLoader;
        private readonly DonatShopItemModelStorage _donatShopItemModelStorage;
        private readonly Dictionary<string, UIDonatItemCellsContainer> _containers;
        private FarmShopInteractor _farmShopInteractor;
        private const string _pageTextKey = "UIDonatShop_";
        private GameObject _offerTemp; // Temp solution

        public DonatShopState(string stateId,
                              IStateMachine stateMachine,
                              IInputController<MainLayer> inputController,
                              IUIService uiService,
                              IInstantiator instantiator,
                              UIRoot uiRoot,
                              IconLoader iconLoader,
                              DonatShopItemModelStorage donatShopItemModelStorage) : base(stateId)
        {
            _stateMachine = stateMachine;
            _inputController = inputController;
            _uiService = uiService;
            _instantiator = instantiator;
            _uiRoot = uiRoot;
            _iconLoader = iconLoader;
            _donatShopItemModelStorage = donatShopItemModelStorage;
            _containers = new Dictionary<string, UIDonatItemCellsContainer>();
        }

        public override void OnEnter(params object[] args)
        {
            _inputController.Lock();
            _uiService.Move<UIBottomWindow>(_uiRoot.TopContainer);
            _uiService.Move<UIHeaderWindow>(_uiRoot.TopContainer);
            var window = _uiService.Show<UIDonateShopWindow>();
            window.CloseEvent += (o, a) =>
            {
                _stateMachine.Exit(Id);
            };
            
            _offerTemp = _instantiator.InstantiatePrefab(window.OfferItemPrefab, window.Content);
            
            foreach (var itemModel in _donatShopItemModelStorage.Get())
            {
                if (!_containers.ContainsKey(itemModel.TypeId))
                {
                    var itemContainer =
                        _instantiator.InstantiatePrefabForComponent<UIDonatItemCellsContainer>(
                            window.ItemsContainerPrefab, window.Content);
                    _containers.Add(itemModel.TypeId, itemContainer);
                    itemContainer.SetItemId(itemModel.TypeId);
                    itemContainer.SetHeaderText(LocalizationManager.Localize(_pageTextKey + itemModel.TypeId));
                }

                var container = _containers[itemModel.TypeId];
                
                var itemCell =
                    _instantiator.InstantiatePrefabForComponent<UIDonatItemCell>(
                        window.ItemCellPrefab, container.Content);
                itemCell.SetItemId(itemModel.ModelId);
                itemCell.SetName("Я предмет");
                itemCell.SetCount(itemModel.Count.ToString());
                itemCell.SetPrice(itemModel.Price + " .руб");
                itemCell.SetItemIcon(_iconLoader.Load(itemModel.IconName));
                var isFree = itemModel.Price <= 0;
                itemCell.SetGiftLabelActive(isFree);
                itemCell.SetBestPriceLabelActive(!isFree && Tools.RandomBool());
                itemCell.SetButtonBuy(!isFree);
                itemCell.SetButtonGift(isFree);
            }
        }

        public override void OnExit()
        {
            foreach (var container in _containers.Values)
            {
                Object.Destroy(container.gameObject);
            }
            Object.Destroy(_offerTemp);
            _offerTemp = null;
            _containers.Clear();
            _uiService.Hide<UIDonateShopWindow>();
            _inputController.UnLock();
            _uiService.Move<UIBottomWindow>(_uiRoot.MiddleContainer);
            _uiService.Move<UIHeaderWindow>(_uiRoot.MiddleContainer);
        }
    }
}