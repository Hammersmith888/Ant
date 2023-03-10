using System;
using BugsFarm.Services.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class OrderRewardItem : MonoBehaviour
    {
        public string ID { get; private set; }
        public event Action<string> OnGetRewardClick;
        public GameObject CurrencyItemPrefab => _currencyItemPrefab;
        public RectTransform CurrencyContainer => _currencyContainer;

        [SerializeField] private Button _getRewardButton;
        [SerializeField] private TextMeshProUGUI _getRewardButtonText;
        [SerializeField] private TextMeshProUGUI _yourRewardText;
        [SerializeField] private RectTransform _currencyContainer;
        [SerializeField] private GameObject _currencyItemPrefab;

        private const string _yourRewardTextKey = "UIOrderBoard_YourReward";
        private const string _getRewardText = "UIOrderBoard_GetReward";
        private bool _initialized;
        public void Initialize(string id)
        {
            if(_initialized) return;
            ID = id;
            _getRewardButton.onClick.AddListener(OnGetRewardClicked);
            _initialized = true;
        }

        public void Dispose()
        {
            if(!_initialized) return;
            _getRewardButton.onClick.RemoveListener(OnGetRewardClicked);
            OnGetRewardClick = null;
            _initialized = false;
        }

        public void SetInteractableGetButton(bool interactable)
        {
            _getRewardButton.interactable = interactable;
        }
        private void Awake()
        {
            _yourRewardText.text = LocalizationManager.Localize(_yourRewardTextKey);
            _getRewardButtonText.text = LocalizationManager.Localize(_getRewardText);
        }

        private void OnGetRewardClicked()
        {
            OnGetRewardClick?.Invoke(ID);
        }
    }
}