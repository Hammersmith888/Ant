using System;
using BugsFarm.Services.SimpleLocalization;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class TerrariumView : BaseView
    {
        public RectTransform ParamsContainer => _paramsContainer;
        public GameObject ParamPrefabItem => _paramPrefabItem;
        public RectTransform MyBugsContainer => _myBugsContainer;
        public GameObject MyBugPrefabItem => _myBugPrefabItem;
        
        [Header("BugInfo")] 
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private CanvasGroup _upgradeGroupButton;
        [SerializeField] private TextMeshProUGUI _infoHeader;
        [SerializeField] private TextMeshProUGUI _upgradeText;
        [SerializeField] private TextMeshProUGUI _upgradePriceText;
        [Space]
        [SerializeField] private Image _upgradeCurrencyPlaceHolder;
        [SerializeField] private Image _iconPlaceHolder;
        [SerializeField] private SlicedFilledImage _expirienceFillProgress;
        [SerializeField] private GameObject _expirienceProgressContainer;
        [Space]
        [SerializeField] private RectTransform _fxRect;
        [SerializeField] private RectTransform _fxArea;
        [SerializeField] private RectTransform _paramsContainer;
        [SerializeField] private GameObject _paramPrefabItem;

        [Space]
        [Header("MyBugs")]
        [SerializeField] private TextMeshProUGUI _myBugsHeader;
        [SerializeField] private RectTransform _myBugsContainer;        
        [SerializeField] private GameObject _myBugPrefabItem;
        private const string _upgradeTextKey = "UIMyBugs_Upgrade";
        private const string _terrariumHeaderTextKey = "UIMyBugs_TerrariumHeader";
        public event EventHandler InfoEvent;
        public event EventHandler UpgradeEvent;
        

        public override void Show()
        {
            base.Show();
            _infoButton.onClick.AddListener(OnInfoEventHandler);
            _upgradeButton.onClick.AddListener(OnUpgradeEventHandler);
            _upgradeText.text = LocalizationManager.Localize(_upgradeTextKey);
            _myBugsHeader.text = LocalizationManager.Localize(_terrariumHeaderTextKey);
        }

        public override void Hide()
        {
            base.Hide();            
            _infoButton.onClick.RemoveListener(OnInfoEventHandler);
            _upgradeButton.onClick.RemoveListener(OnUpgradeEventHandler);
            ResetEvents();
        }

        public void SetUpgradePriceText(string value)
        {
            _upgradePriceText.text = value;
        }
        
        public void SetInfoHeaderText(string value)
        {
            _infoHeader.text = value;
        }

        public void SetExpirienceProgress(float progress01)
        {
            _expirienceFillProgress.fillAmount = progress01;
            _fxRect.anchoredPosition = _fxRect.anchoredPosition.SetX(_fxArea.rect.width * progress01);
            _fxRect.gameObject.SetActive(progress01 > 0 && progress01 < 1);
        }
        
        public void SetExpirienceProgressActive(bool value)
        {
            _expirienceProgressContainer.SetActive(value);
        }

        public void SetAvatarIcon(Sprite value)
        {
            var color = _iconPlaceHolder.color;
            color.a = value ? 1 : 0;
            _iconPlaceHolder.color = color;
            _iconPlaceHolder.sprite = value;
        }
        
        public void SetUpgradeCurrencyIcon(Sprite value)
        {
            _upgradeCurrencyPlaceHolder.sprite = value;
        }

        public void SetUpgradeInteractable(bool value)
        {
            _upgradeGroupButton.interactable = value;
            _upgradeGroupButton.alpha = value ? 1 : 0.5f;
        }

        public void SetUpgradeButtonActive(bool value)
        {
            _upgradeGroupButton.gameObject.SetActive(value);
        }
        
        public void SetInfoButtonActive(bool value)
        {
            _infoButton.gameObject.SetActive(value);
        }

        public void ResetEvents()
        {
            InfoEvent = null;
            UpgradeEvent = null;
        }

        private void OnInfoEventHandler()
        {
            InfoEvent?.Invoke(this,EventArgs.Empty);
        }
        private void OnUpgradeEventHandler()
        {
            UpgradeEvent?.Invoke(this,EventArgs.Empty);
        }
    }
}