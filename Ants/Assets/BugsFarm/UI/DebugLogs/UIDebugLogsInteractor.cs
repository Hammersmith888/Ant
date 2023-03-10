using BugsFarm.Services.UIService;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class UIDebugLogsInteractor
    {
        private readonly IUIService _uiService;

        private readonly IInstantiator _instantiator;

        private readonly UIRoot _uiRoot;

        private GameObject _logger;

        public UIDebugLogsInteractor(IUIService uiService,
                                     IInstantiator instantiator,
                                     UIRoot uiRoot)
        {
            _uiService = uiService;
            _instantiator = instantiator;
            _uiRoot = uiRoot;
        }

        public void Initialize()
        {
            var window = _uiService.Show<UIDebugLogsWindow>(_uiRoot.MiddleContainer);
            window.OnClick += OnWindowClick;
        }

        public void Dispose()
        {
            _uiService.Hide<UIDebugLogsWindow>();
            Object.Destroy(_logger);
        }

        private void OnWindowClick()
        {
            var window = _uiService.Get<UIDebugLogsWindow>();
            if (!_logger)
            {
                _logger = _instantiator.InstantiatePrefab(window.Prefab);
                _logger.SetActive(false);
            }

            var show = !_logger.activeSelf;
            _logger.SetActive(show);
            window.SetTextColor(show ? Color.green : window.OriginalColor);
        }
    }
}