using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIYesNoWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private TextMeshProUGUI _buttonYesText;
        [SerializeField] private TextMeshProUGUI _buttonNoText;
        [SerializeField] private Button _buttonYes;
        [SerializeField] private Button _buttonNo;
        [SerializeField] private Button[] _buttonsClose;
        
        private const string _buttonYesTextKey = "UIYesNo_Yes";
        private const string _buttonNoTextKey = "UIYesNo_No";

        public event EventHandler YesEvent;
        public event EventHandler NoEvent;
        
        public override void Show()
        {
            base.Show();
            
            foreach (var button in _buttonsClose)
            {
                button.onClick.AddListener(Close);
            }
            _buttonYes.onClick.AddListener(YesEventHandler);
            _buttonNo.onClick.AddListener(NoEventHandler);
            
            _buttonYesText.text = LocalizationManager.Localize(_buttonYesTextKey);
            _buttonNoText.text = LocalizationManager.Localize(_buttonNoTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var button in _buttonsClose)
            {
                button.onClick.RemoveListener(Close);
            }
            _buttonYes.onClick.RemoveListener(YesEventHandler);
            _buttonNo.onClick.RemoveListener(NoEventHandler);
            
            YesEvent = null;
            NoEvent = null;
        }
        
        public void SetText(string message)
        {
            _messageText.text = message;
        }
        
        private void YesEventHandler()
        {
            YesEvent?.Invoke(this, EventArgs.Empty);
        }

        private void NoEventHandler()
        {
            NoEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}