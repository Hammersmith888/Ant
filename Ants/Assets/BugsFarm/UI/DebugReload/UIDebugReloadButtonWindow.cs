using System;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDebugReloadButtonWindow : UISimpleWindow
    {
        public event Action OnButtonClicked;
        
        [SerializeField] private Button reloadButton;

        public override void Show()
        {
            base.Show();
            reloadButton.onClick.AddListener(NotifyKillingButtonPressed);
        }

        private void NotifyKillingButtonPressed()
        {
            OnButtonClicked?.Invoke();
        }

        public override void Hide()
        {
            base.Hide();
            reloadButton.onClick.RemoveListener(NotifyKillingButtonPressed);
        }
    }

    public class UIDebugReloadInteractor
    {
        private readonly GameReloader _gameReloader;
        private UIDebugReloadButtonWindow _window;
        private readonly IUIService _uiService;

        public UIDebugReloadInteractor(IUIService uiService,
                                        GameReloader gameReloader)
        {
            _gameReloader = gameReloader;
            _uiService = uiService;
        }

        public void Initialize()
        {
            _window = _uiService.Show<UIDebugReloadButtonWindow>();
            _window.OnButtonClicked += StartReloadingGame;
        }

        private void StartReloadingGame()
        {
            _window.OnButtonClicked -= StartReloadingGame;
            _gameReloader.ReloadGame();
        }
    }
}