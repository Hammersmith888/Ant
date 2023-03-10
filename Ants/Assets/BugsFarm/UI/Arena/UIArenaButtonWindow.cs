using System;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI.Arena
{
    public class UIArenaButtonWindow : UISimpleWindow
    {
        [SerializeField] private Button _button;
        
        public event EventHandler ClickEvent;

        public override void Show()
        {
            base.Show();
            
            _button.onClick.AddListener(ClickEventHandler);
        }

        public override void Hide()
        {
            base.Hide();
            
            _button.onClick.RemoveListener(ClickEventHandler);
            
            ClickEvent = null;
        }
        
        private void ClickEventHandler()
        {
            ClickEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
