using System;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIBugInfoMenuWindow : UISimpleWindow
    {
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private Button _actionButton;
        [SerializeField] private Button _arrowLeftButton;
        [SerializeField] private Button _arrowRightButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _replaceButton;
        [SerializeField] private Button _removeButton;
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _typeLabel;
        [SerializeField] private TextMeshProUGUI _statLabel;
        [SerializeField] private TextMeshProUGUI _actionButtonText;
        [SerializeField] private TextMeshProUGUI _upgradeButtonText;
        [SerializeField] private TextMeshProUGUI _upgradeButtonCount;
        [SerializeField] private Image _iconImage;
        
        public event EventHandler SetTaskEvent;
        public event EventHandler ArrowLeftEvent;
        public event EventHandler ArrowRightEvent;
        public event EventHandler UpgradeEvent;
        public event EventHandler ReplaceEvent;
        public event EventHandler RemoveEvent;

        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _actionButton.onClick.AddListener(SetTaskEventHandler);
            _arrowLeftButton.onClick.AddListener(ArrowLeftEventHandler);
            _arrowRightButton.onClick.AddListener(ArrowRightEventHandler);
            _upgradeButton.onClick.AddListener(UpgradeEventHandler);
            _replaceButton.onClick.AddListener(ReplaceEventHandler);
            _removeButton.onClick.AddListener(RemoveEventHandler);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _actionButton.onClick.RemoveListener(SetTaskEventHandler);
            _arrowLeftButton.onClick.RemoveListener(ArrowLeftEventHandler);
            _arrowRightButton.onClick.RemoveListener(ArrowRightEventHandler);
            _upgradeButton.onClick.RemoveListener(UpgradeEventHandler);
            _replaceButton.onClick.RemoveListener(ReplaceEventHandler);
            _removeButton.onClick.RemoveListener(RemoveEventHandler);
            
            SetTaskEvent = null;
            ArrowLeftEvent = null;
            ArrowRightEvent = null;
            UpgradeEvent = null;
            ReplaceEvent = null;
            RemoveEvent = null;
        }
        
        public void SetHeader(string header)
        {
            _headerLabel.text = header;
        }

        public void SetIcon(Sprite iconSprite)
        {
            _iconImage.sprite = iconSprite;
        }

        public void SetTypeName(string typeName)
        {
            _typeLabel.text = typeName;
        }

        public void SetStat(string value)
        {
            _statLabel.text = value;
        }

        public void SetTaskLabel(string task)
        {
            _actionButtonText.text = task;
        }
        public void SetUpgradeCost(string value)
        {
            _upgradeButtonCount.text = value;
        }

        private void SetTaskEventHandler()
        {
            SetTaskEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ArrowLeftEventHandler()
        {
            ArrowLeftEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ArrowRightEventHandler()
        {
            ArrowRightEvent?.Invoke(this, EventArgs.Empty);
        }

        private void UpgradeEventHandler()
        {
            UpgradeEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ReplaceEventHandler()
        {
            ReplaceEvent?.Invoke(this, EventArgs.Empty);
        }

        private void RemoveEventHandler()
        {
            RemoveEvent?.Invoke(this, EventArgs.Empty);
        }

        public void SetActiveAssignTab(bool isActive)
        {
            _actionButton.gameObject.SetActive(isActive);
            _arrowLeftButton.gameObject.SetActive(isActive);
            _arrowRightButton.gameObject.SetActive(isActive);
        }
    }
}