using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class HospitalItemSlot : MonoBehaviour
    {
        public string Id { get; private set; }

        public Color InitLevelColor { get; private set; }

        [SerializeField] private float _progressOffset = 0.25f;
        [SerializeField] private Image _iconPaceHolder;
        [SerializeField] private Image _progressFill;
        [SerializeField] private Image _lifeTimeFill;
        [SerializeField] private GameObject _lifeTimeContainer;
        [SerializeField] private GameObject _progressContainer;
        [SerializeField] private TextMeshProUGUI _progressText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private CanvasGroup _buyButtonGroup;
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _removeButton;

        public event EventHandler<string> BuyEvent;
        public event EventHandler<string> RemoveEvent;

        public void Init(string id)
        {
            if (string.IsNullOrEmpty(Id))
            {
                _buyButton.onClick.AddListener(OnBuyEvent);
                _removeButton.onClick.AddListener(OnRemoveEvent);
            }

            Id = id;
            InitLevelColor = _levelText.color;
        }

        public void SetLifeTimeActive(bool value)
        {
            _lifeTimeContainer.SetActive(value);
        }

        public void SetBuyButtonActive(bool value)
        {
            _buyButton.gameObject.SetActive(value);
        }

        public void SetRemoveButtonActive(bool value)
        {
            _removeButton.gameObject.SetActive(value);
        }

        public void SetBuyButtonInteractable(bool interactable)
        {
            _buyButtonGroup.interactable = interactable;
            _buyButtonGroup.alpha = interactable ? 1 : 0.5f;
        }

        public void SetRepairProgressActive(bool value)
        {
            _progressContainer.SetActive(value);
        }

        public void SetRepairProgress(float progress01)
        {
            if (_progressOffset > 0)
            {
                _progressFill.fillAmount = ((1f - _progressOffset) * (progress01 / 1f)) + _progressOffset;
            }
            else
            {
                _progressFill.fillAmount = progress01;
            }
        }

        public void SetRepairProgressText(string value)
        {
            _progressText.text = value;
        }

        public void SetLifeTimeProgress(float progress01)
        {
            _lifeTimeFill.fillAmount = progress01;
        }

        public void SetLifeTimeColor(Color value)
        {
            value.a = _lifeTimeFill.color.a;
            _lifeTimeFill.color = value;
        }

        public void SetPriceText(string value)
        {
            _priceText.text = value;
        }

        public void SetLevelText(string value)
        {
            _levelText.text = value;
        }
        
        public void SetLevelColor(Color color)
        {
            color.a = _levelText.color.a;
            _levelText.color = color;
        }

        public void SetIcon(Sprite value)
        {
            _iconPaceHolder.sprite = value;
        }

        private void OnDestroy()
        {
            _buyButton.onClick.RemoveListener(OnBuyEvent);
            _removeButton.onClick.RemoveListener(OnRemoveEvent);
            BuyEvent = null;
            RemoveEvent = null;
        }

        private void OnBuyEvent()
        {
            BuyEvent?.Invoke(this, Id);
        }

        private void OnRemoveEvent()
        {
            RemoveEvent?.Invoke(this, Id);
        }
    }
}