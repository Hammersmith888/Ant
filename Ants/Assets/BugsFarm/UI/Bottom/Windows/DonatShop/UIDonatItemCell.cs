using System;
using BugsFarm.Services.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDonatItemCell : MonoBehaviour
    {
        public string Id { get; private set; }
        
        [SerializeField] private TextMeshProUGUI _nameLabel;
        [SerializeField] private TextMeshProUGUI _countLabel;
        [SerializeField] private TextMeshProUGUI _buttonBuyLabel;
        [SerializeField] private TextMeshProUGUI _buttonGiftLabel;
        [SerializeField] private TextMeshProUGUI _giftLabel;
        [SerializeField] private TextMeshProUGUI _bestPriceLabel;
        [SerializeField] private Button _buttonBuy;
        [SerializeField] private Button _buttonGift;
        [SerializeField] private Image _itemIcon;

        public event EventHandler<string> BuyButtonClickEvent;
        public event EventHandler<string> GiftButtonClickEvent;

        private const string _giftButtonLabelTextKey = "UIDonatShop_GiftButton";
        private const string _giftLabelTextKey = "UIDonatShop_GiftLabel";
        private const string _bestPriceTextKey = "UIDonatShop_BestPriceLabel";
        
        private void Awake()
        {
            _buttonGiftLabel.text = LocalizationManager.Localize(_giftButtonLabelTextKey);
            _giftLabel.text = LocalizationManager.Localize(_giftLabelTextKey);
            _bestPriceLabel.text = LocalizationManager.Localize(_bestPriceTextKey);
            
            _buttonBuy.onClick.AddListener(OnBuyButtonClickEventHandler); 
            _buttonGift.onClick.AddListener(OnGiftButtonClickEventHandler); 
        }

        public void SetItemId(string id)
        {
            Id = id;
        }
        
        public void SetName(string nameValue)
        {
            _nameLabel.text = nameValue;
        }

        public void SetPrice(string priceValue)
        {
            _buttonBuyLabel.text = priceValue;
        }

        public void SetCount(string countValue)
        {
            _countLabel.text = countValue;
        }

        public void SetButtonGift(bool value)
        {
            _buttonGift.gameObject.SetActive(value);
        }

        public void SetButtonBuy(bool value)
        {
            _buttonBuy.gameObject.SetActive(value);
        }

        public void SetGiftLabelActive(bool value)
        {
            _giftLabel.transform.parent.gameObject.SetActive(value);
        }

        public void SetBestPriceLabelActive(bool value)
        {
            _bestPriceLabel.transform.parent.gameObject.SetActive(value);
        }

        public void SetItemIcon(Sprite icon)
        {
            _itemIcon.sprite = icon;
        }

        private void OnDestroy()
        {
            _buttonBuy.onClick.RemoveListener(OnBuyButtonClickEventHandler);    
            _buttonGift.onClick.RemoveListener(OnGiftButtonClickEventHandler); 

            BuyButtonClickEvent = null;
            GiftButtonClickEvent = null;
        }
        
        private void OnBuyButtonClickEventHandler()
        {
            BuyButtonClickEvent?.Invoke(this, Id);
        }

        private void OnGiftButtonClickEventHandler()
        {
            GiftButtonClickEvent?.Invoke(this, Id);
        }
    }
}