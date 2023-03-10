using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIOrderBoard : UISimpleWindow
    {
        [Header("Animation")] 
        [SerializeField] private UIWindowPopupAnimation _animation;

        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI _windowHeaderText;
        [SerializeField] private TextMeshProUGUI _dealerDialogText;
        [SerializeField] private TextMeshProUGUI _specialOrderText;

        [Header("Buttons")] 
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button[] _closeButtons;

        [Header("Settings")] 
        [SerializeField] private RectTransform _baseOrderItemsContainer;
        [SerializeField] private RectTransform _baseOrderContainer;
        [SerializeField] private RectTransform _specialOrderContainer;

        [Header("Prefabs")] 
        [SerializeField] private GameObject _baseItemPrefab;
        [SerializeField] private GameObject _specialItemPrefab;
        [SerializeField] private GameObject _rewardItemPrefab;
        [SerializeField] private GameObject _orderNextPrefab;
        [SerializeField] private GameObject _baseOrderTimerPrefab;
        [SerializeField] private GameObject _specialOrderTimerPrefab;

        private const string _windowHeaderTextKey = "UIOrderBoard_Header";
        private const string _specialOrderTextKey = "UIOrderBoard_SpecialOrder";
        
        public event EventHandler OnInfoClicked;
        public event EventHandler OnCloseClicked;
        





        public GameObject SpecialOrderItemPrefab => _specialItemPrefab;
        public GameObject BaseOrderItemPrefab => _baseItemPrefab;
        public GameObject RewardItemPrefab => _rewardItemPrefab;
        public GameObject OrderNextItemPrefab => _orderNextPrefab;
        public GameObject BaseOrderTimerPrefab => _baseOrderTimerPrefab;  
        public GameObject SpecialOrderTimerPrefab => _specialOrderTimerPrefab;
        public RectTransform SpecialOrderContainer => _specialOrderContainer;
        public RectTransform BaseOrderContainer => _baseOrderContainer;
        public RectTransform BaseOrderItemsContainer => _baseOrderItemsContainer;

        public void SetDealerDialogText(string text)
        {
            if (!_dealerDialogText)
            {
                Debug.LogError($"{this} : TextMeshProUGUI _dealerDialogText is missing");
                return;
            }

            _dealerDialogText.text = text;
        }

        public override void Show()
        {
            base.Show();
            
            _windowHeaderText.text = LocalizationManager.Localize(_windowHeaderTextKey);
            _specialOrderText.text = LocalizationManager.Localize(_specialOrderTextKey);
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(OnCloseClick);
            }

            _infoButton.onClick.AddListener(OnInfoClick);
        }

        public override void Hide()
        {
            base.Hide();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(OnCloseClick);
            }

            _infoButton.onClick.RemoveListener(OnInfoClick);
            OnCloseClicked = null;
            OnInfoClicked = null;
           
        }
        private void OnCloseClick()
        {
            OnCloseClicked?.Invoke(this, EventArgs.Empty);
        }
         private void OnInfoClick()
        {
            OnInfoClicked?.Invoke(this, EventArgs.Empty);
        }
    }
}