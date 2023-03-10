using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class CatalogItem : MonoBehaviour
    {
        public RectTransform ParamsContainer => _paramsContainer;
        public GameObject ParamItemPrefab => _paramItemPrefab;
        
        [Header("Information")]
        [SerializeField] private Image _iconPlaceHolder;
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private RectTransform _paramsContainer;
        [SerializeField] private GameObject _paramItemPrefab;
        [SerializeField] private CanvasGroup _itemGroupe;

        [Header("Can buy")] 
        [SerializeField] private GameObject _canBuyContainer;
        [SerializeField] private TextMeshProUGUI _textPrice;
        [SerializeField] private Image _iconPrice;
        [SerializeField] private Button _buttonBuy;
        [SerializeField] private Button _infoButton;

        [Header("Low level")] 
        [SerializeField] private GameObject _lowLevelContainer;
        [SerializeField] private TextMeshProUGUI _fromLevelBuyText;
        public event EventHandler<string> BuyClickEvent;
        public event EventHandler<string> InfoEvent;
        
        public string Id { get; set; }

        private void Awake()
        {
            _buttonBuy.onClick.AddListener(OnBuyButtonPressHandler);
            _infoButton.onClick.AddListener(OnInfoEventHandler);
        }

        private void OnDestroy()
        {
            _buttonBuy.onClick.RemoveListener(OnBuyButtonPressHandler);
            _infoButton.onClick.RemoveListener(OnInfoEventHandler);
            BuyClickEvent = null;
            InfoEvent = null;
        }

        public void Clear()
        {
            _buttonBuy.onClick.RemoveAllListeners();
        }

        public void SetIcon(Sprite spriteIcon)
        {
            _iconPlaceHolder.sprite = spriteIcon;
        }
        
        public void SetHeader(string header)
        {
            _headerText.text = header;
        }

        public void SetPrice(string price)
        {
            _textPrice.text = price;
        }

        public void SetPriceIcon(Sprite priceIcon)
        {
            _iconPrice.sprite = priceIcon;
        }

        public void SetCanBuy(bool value)
        {
            _canBuyContainer.SetActive(value);
            _lowLevelContainer.SetActive(!value);
        }

        public void SetInteractable(bool value)
        {
            _itemGroupe.interactable = value;
            _itemGroupe.alpha = value ? 1 : 0.5f;
        }
        
        public void SetLowLevelText(string text)
        {
            _fromLevelBuyText.text = text;
        }
        
        private void OnBuyButtonPressHandler()
        {
            BuyClickEvent?.Invoke(this, Id);
        }

        private void OnInfoEventHandler()
        {
            InfoEvent?.Invoke(this, Id);
        }
    }
}