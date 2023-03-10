using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIAllUnitsResurrectionWindow : UISimpleWindow
    {
        [SerializeField] private Button _startOverButton;
        [SerializeField] private Button _resurrectButton;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _startOverButtonText;
        [SerializeField] private TextMeshProUGUI _resurrectButtonText;
        [SerializeField] private TextMeshProUGUI _resurrectButtonCount;

        public event EventHandler StartOverEvent;
        public event EventHandler ResurrectEvent;
        public event EventHandler<EventArgs> ShopEvent;

        
        public override void Show()
        {
            base.Show();
            
            _startOverButton.onClick.AddListener(StartOverEventHandler);
            _resurrectButton.onClick.AddListener(ResurrectEventHandler);
            
        }

        public void SetDescription(string description) => _descriptionText.text = description;
        public void SetTextToStartOverButton(string startOverText) => _startOverButtonText.text = startOverText;
        public void SetTextToResurrectButton(string resurrectText) => _resurrectButtonText.text = resurrectText;

        public override void Hide()
        {
            base.Hide();
            
            _startOverButton.onClick.RemoveListener(StartOverEventHandler);
            _resurrectButton.onClick.RemoveListener(ResurrectEventHandler);
            
            StartOverEvent = null;
            ResurrectEvent = null;
        }
                
        public void SetPrice(string value)
        {
            _resurrectButtonCount.text = value;
        }

        public void OpenDonateWindow()
        {
            ShopEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void StartOverEventHandler()
        {
            StartOverEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ResurrectEventHandler()
        {
            ResurrectEvent?.Invoke(this, EventArgs.Empty);
        }

    }
}
