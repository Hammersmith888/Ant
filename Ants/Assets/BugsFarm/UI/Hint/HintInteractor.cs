using System;
using BugsFarm.Services.UIService;
using UnityEngine;

namespace BugsFarm.UI
{
    public class HintInteractor
    {
        private readonly IUIService _uiService;
        private Action _onClose;

        public HintInteractor(IUIService uiService)
        {
            _uiService = uiService;
            PrepareWindow();
        }

        public void SetHint(string hintText)
        {
            var window = _uiService.Get<UIHintWindow>();
            window.SetHint(hintText);
        }
        
        public void SetHeader(string header)
        {
            var window = _uiService.Get<UIHintWindow>();
            window.SetHeader(header);
        }
        
        public void SetIcon(Sprite sprite)
        {
            var window = _uiService.Get<UIHintWindow>();
            window.SetIcon(sprite);
        }

        public void SetAction(Action action)
        {
            _onClose = action;
        }
        
        private void PrepareWindow()
        {
            var window = _uiService.Show<UIHintWindow>();
            window.CloseEvent += OnClose;
        }
        
        private void FinalizeHint()
        {
            _uiService.Hide<UIHintWindow>();
            _onClose = null;
        }
        
        private void OnClose(object sender, EventArgs e)
        {
            FinalizeHint();
            _onClose?.Invoke();
        }
    }
}