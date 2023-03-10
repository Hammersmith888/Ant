using System;
using System.Collections.Generic;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    [RequireComponent(typeof(UIBaseAnimation))]
    public class UIHeaderWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _daysCountText;
        [SerializeField] private TextMeshProUGUI _levelTitleText;
        [SerializeField] private Button _shopButtonCoins;
        [SerializeField] private Button _shopButtonCrystals;
        [SerializeField] private Button _communityButton;
        [SerializeField] private CurrencyView[] _currencyViews;
        
        public event EventHandler CommunityEvent;
        public event EventHandler ShopEvent;

        private string _uiHeaderLevelKey = "UIHeader_LevelTitle";
        
        public IEnumerable<ICurrencyView> CurrencyItems => _currencyViews;
        
        public void ChangeDaysCountText(string text)
        {
            if (!_daysCountText)
            {
                Debug.LogError($" {nameof(ChangeDaysCountText)} :: {nameof(_daysCountText)} is Null", this);
                return;
            }

            _daysCountText.text = text;
        }

        public override void Show()
        {
            base.Show();
            _communityButton.onClick.AddListener(OnCommunityClicked);
            _shopButtonCoins.onClick.AddListener(OnShopClicked);
            _shopButtonCrystals.onClick.AddListener(OnShopClicked);
            _levelTitleText.text = LocalizationManager.Localize(_uiHeaderLevelKey);
        }

        public override void Hide()
        {
            base.Hide();
            _communityButton.onClick.RemoveListener(OnCommunityClicked);
            _shopButtonCoins.onClick.RemoveListener(OnShopClicked);
            _shopButtonCrystals.onClick.RemoveListener(OnShopClicked);
            CommunityEvent = null;
            ShopEvent = null;
        }

        private void OnShopClicked()
        {
            ShopEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnCommunityClicked()
        {
            CommunityEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}