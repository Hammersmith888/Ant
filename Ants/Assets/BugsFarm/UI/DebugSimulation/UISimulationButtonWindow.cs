using System;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISimulationButtonWindow : UISimpleWindow
    {
        [SerializeField] private Button _button;
        
        public event EventHandler ClickEvent;
        
        public override void Show()
        {
            base.Show();

            _button.onClick.AddListener(OnClickEventHandler);
        }

        public override void Hide()
        {
            base.Hide();

            _button.onClick.RemoveListener(OnClickEventHandler);
        }

        private void OnClickEventHandler()
        {
            ClickEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
