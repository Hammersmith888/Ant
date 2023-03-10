using System;
using System.Collections.Generic;
using BugsFarm.Services.UIService;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDebugDeathWindow : UISimpleWindow
    {
        public event Action OnButtonClicked;
        
        [SerializeField] private Button killAntButton;

        public override void Show()
        {
            base.Show();
            killAntButton.onClick.AddListener(NotifyKillingButtonPressed);
        }

        private void NotifyKillingButtonPressed()
        {
            OnButtonClicked?.Invoke();
        }

        public override void Hide()
        {
            base.Hide();
            killAntButton.onClick.RemoveListener(NotifyKillingButtonPressed);
        }
    }
}
