using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using UniRx;

namespace BugsFarm.UI
{
    public class UIHeaderInteractor : IInteractorService
    {
        public string Id => "UIHeaderInteractor";

        private readonly UIRoot _uiRoot;
        private readonly IUIService _uiService;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IUser _user;
        private IDisposable _currencyChangedEvent;
        private Dictionary<string, ICurrencyView> _views;
        private IDisposable _levelChangedEvent;

        public UIHeaderInteractor(UIRoot uiRoot,
                                  IUIService uiService,
                                  IInputController<MainLayer> inputController,
                                  IUser user)
        {
            _uiRoot = uiRoot;
            _uiService = uiService;
            _inputController = inputController;
            _user = user;
        }

        public void Init()
        {
            var window = _uiService.Show<UIHeaderWindow>();
            var resurrectionWindow = _uiService.Get<UIAllUnitsResurrectionWindow>();
            
            window.CommunityEvent += OnCommunityClicked;
            window.ShopEvent += OnShopClicked;
            resurrectionWindow.ShopEvent += OnShopClicked;

            _currencyChangedEvent = MessageBroker.Default
                .Receive<UserCurrencyChangedProtocol>()
                .Subscribe(protocol => UpdateCurrency(protocol.Currency));

            _levelChangedEvent = MessageBroker.Default.Receive<UserLevelChangedProtocol>().Subscribe(OnUserLevelChanged);

            window.ChangeDaysCountText(_user.GetLevel().ToString());
            _views = window.CurrencyItems.ToDictionary(x => x.CurrencyID);
            foreach (var currency in _user.GetCurrency())
            {
                UpdateCurrency(currency);
            }
        }

        private void OnUserLevelChanged(UserLevelChangedProtocol protocol)
        {
            _uiService.Get<UIHeaderWindow>().ChangeDaysCountText(protocol.Level.ToString());
        }
        public void Dispose()
        {
            _currencyChangedEvent?.Dispose();
            _levelChangedEvent.Dispose();
            _currencyChangedEvent = null;
            _uiService.Hide<UIHeaderWindow>();
        }

        private void UpdateCurrency(CurrencyModel currencyModel)
        {
            if (!_views.ContainsKey(currencyModel.ModelID))
            {
                throw new InvalidOperationException($"Currency view with Id : {currencyModel.ModelID}" +
                                                    ", does not presented.");
            }

            var view = _views[currencyModel.ModelID];
            view.SetCurrencyText(currencyModel.Count.ToString());
        }
        
        private void OnShopClicked(object sender, EventArgs e)
        {
            MessageBroker.Default.Publish(new SwitchDonatShopProtocol());
        }

        private void OnCommunityClicked(object sender, EventArgs e)
        {
            if (_inputController.Locked) return;
            _inputController.Lock();

            var window = _uiService.Show<UICommunityWindow>();
            window.CloseEvent += OnCommunityClose;
        }

        private void OnCommunityClose(object sender, EventArgs e)
        {
            _uiService.Hide<UICommunityWindow>();
            _inputController.UnLock();
        }
    }
}