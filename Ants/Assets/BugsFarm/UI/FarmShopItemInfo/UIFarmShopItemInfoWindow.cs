using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIFarmShopItemInfoWindow : UISimpleWindow
    {
        [SerializeField] private Button _confirmButton;
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _confirmButtonText;
        [SerializeField] private TextMeshProUGUI _confirmButtonCount;
        [SerializeField] private TextMeshProUGUI _cancelButtonText;
        [SerializeField] private Image _iconImage;
        
        private const string _cancelButtonTextKey = "UIFarmShopItemInfo_CancelButton";
        private const string _confirmButtonTextKey = "UIFarmShopItemInfo_ConfirmButton";

        public event EventHandler ConfirmEvent;
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _confirmButton.onClick.AddListener(ConfirmEventHandler);
            
            _cancelButtonText.text = LocalizationManager.Localize(_cancelButtonTextKey);
            _confirmButtonText.text = LocalizationManager.Localize(_confirmButtonTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _confirmButton.onClick.RemoveListener(ConfirmEventHandler);
            
            ConfirmEvent = null;
        }
                
        public void SetHeader(string header)
        {
            _headerText.text = header;
        }

        public void SetDescription(string text)
        {
            _descriptionText.text = text;
        }
         
        public void SetIcon(Sprite iconSprite)
        {
            _iconImage.sprite = iconSprite;
        }
       
        public void SetItemPrice(string value)
        {
            _confirmButtonCount.text = value;
        }
      
        private void ConfirmEventHandler()
        {
            ConfirmEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
