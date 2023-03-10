using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIFarmShopCellView : MonoBehaviour
    {
        public event Action<string> OnByClicked;
        public event Action<string> OnInfoClicked;
        public string ID { get; private set; }
        public int SortCost { get; set; }

        [SerializeField] private Image _icon;
        [SerializeField] private CanvasGroup _infoButtonGorup;
        [SerializeField] private CanvasGroup _buyButtonGorup;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private TextMeshProUGUI _buyPriceText;
        [SerializeField] private TextMeshProUGUI _lokedText;
        [SerializeField] private TextMeshProUGUI _countText;

        public void Init(string id)
        {
            ID = id;
            _buyButton.onClick.AddListener(OnByClick);
            _infoButton.onClick.AddListener(OnInfoClick);
        }

        public void SetBuyButtonActive(bool value)
        {
            _buyButton.gameObject.SetActive(value);
        }

        public void SetBuyInteractable(bool value)
        {
            _buyButtonGorup.interactable = value;
        }

        public void SetBuyAlpha(float alpha01)
        {
            _buyButtonGorup.alpha = alpha01;
        }

        public void SetBuyCostText(string text)
        {
            if (!_buyPriceText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _price : does not exist");
                return;
            }

            _buyPriceText.text = text;
        }

        public void SetInfoIcon(Sprite sprite)
        {
            if (!_icon)
            {
                Debug.LogError($"{this} : Image _icon : does not exist");
            }

            _icon.sprite = sprite;
        }

        public void SetInfoInteractable(bool value)
        {
            _infoButtonGorup.interactable = value;
        }

        public void SetInfoAlpha(float alpha01)
        {
            if (!_icon)
            {
                Debug.LogError($"{this} : Image _icon : does not exist");
            }

            _infoButtonGorup.alpha = alpha01;
        }

        public void SetCountText(string text)
        {
            if (!_countText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _countText : does not exist");
                return;
            }

            _countText.text = text;
        }

        public void SetCountActive(bool value)
        {
            if (!_countText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _countText : does not exist");
                return;
            }

            _countText.transform.parent.gameObject.SetActive(value);
        }

        public void SetLockActive(bool value)
        {
            _lokedText.gameObject.SetActive(value);
        }

        public void SetLokedText(string text)
        {
            _lokedText.text = text;
        }

        private void OnDestroy()
        {
            if (_buyButton)
            {
                _buyButton.onClick.RemoveListener(OnByClick);
            }

            if (_infoButton)
            {
                _infoButton.onClick.RemoveListener(OnInfoClick);
            }

            OnByClicked = null;
            OnInfoClicked = null;
        }

        private void OnByClick()
        {
            OnByClicked?.Invoke(ID);
        }

        private void OnInfoClick()
        {
            OnInfoClicked?.Invoke(ID);
        }
    }
}