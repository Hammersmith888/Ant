using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIRoomBuyWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _buyText;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _yesText;
        [SerializeField] private TextMeshProUGUI _noText;
        [SerializeField] private TextMeshProUGUI _itemAsText;
        [SerializeField] private Button _buttonYes;
        [SerializeField] private Button _buttonNo;
        [SerializeField] private Button[] _buttonsClose;
        [SerializeField] private Transform _itemContainer;
        [SerializeField] private CurrencyItem _itemPrefab;
        
        private const string _buyTextKey = "UIRoomBuy";
        private const string _yesTextKey = "Yes";
        private const string _backTextKey = "Back";
        
        public event EventHandler YesEvent;
        public event EventHandler NoEvent;
        
        public Transform ItemContainer => _itemContainer;
        public CurrencyItem ItemPrefab => _itemPrefab;

        public override void Show()
        {
            base.Show();
            
            foreach (var button in _buttonsClose)
            {
                button.onClick.AddListener(Close);
            }
            _buttonYes.onClick.AddListener(YesEventHandler);
            _buttonNo.onClick.AddListener(NoEventHandler);
            
            _buyText.text = LocalizationManager.Localize(_buyTextKey);
            _yesText.text = LocalizationManager.Localize(_yesTextKey);
            _noText.text = LocalizationManager.Localize(_backTextKey);
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
        
        public void SetHeader(string header)
        {
            if (!_headerText) return;
            _headerText.text = header;
        }

        public void SetDefaultItem(bool active)
        {
            _itemAsText.gameObject.SetActive(active);
        }
        public void SetDefaultItemText(string text)
        {
            _itemAsText.text = text;
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