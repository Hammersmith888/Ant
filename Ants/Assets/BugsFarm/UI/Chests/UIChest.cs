using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using CurrencyItem = BugsFarm.CurrencySystem.CurrencyItem;

namespace BugsFarm.UI
{
    public class UIChest : UISimpleWindow
    {
        [SerializeField] private Transform _contentContainer;
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _buttonLabel;
        [SerializeField] private SkeletonGraphic _spinePlaceHolder;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _closeButton;
        [SerializeField] private CurrencyItem _prefab;

        public event EventHandler ActionButtonEvent;

        public Transform ContentContainer => _contentContainer;
        public SkeletonGraphic SpinePlaceHolder => _spinePlaceHolder;
        public CurrencyItem CurrencyItemPrfab => _prefab;
        
        public override void Show()
        {
            base.Show();
            
            _actionButton.onClick.AddListener(ActionButtonEventHandler);
            _closeButton.onClick.AddListener(Close);
        }

        public override void Hide()
        {
            base.Hide();
            
            _actionButton.onClick.RemoveListener(ActionButtonEventHandler);
            _closeButton.onClick.RemoveListener(Close);
        }

        public void ChangeButtonActivity(bool active)
        {
            _actionButton.gameObject.SetActive(active);
        }

        public void ChangeButtonLabel(string text)
        {
            _buttonLabel.text = LocalizationManager.Localize(text);
        }

        public void ChangeHeaderLabel(string text)
        {
            _headerLabel.text = LocalizationManager.Localize(text);
        }

        public void ChangeSkeletonAsset(SkeletonDataAsset skeletonDataAsset)
        {
            _spinePlaceHolder.startingAnimation = "";
            _spinePlaceHolder.skeletonDataAsset = skeletonDataAsset;
            _spinePlaceHolder.Initialize(true);
            _spinePlaceHolder.SetMaterialDirty();
        }

        public void ChangeGraphicScale(Vector2 localSacle)
        {
            _spinePlaceHolder.transform.localScale = localSacle + Vector2.one; // by default need add one scale 
        }

        private void ActionButtonEventHandler()
        {
            ActionButtonEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}