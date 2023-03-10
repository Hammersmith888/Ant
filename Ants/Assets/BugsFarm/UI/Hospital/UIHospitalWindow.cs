using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIHospitalWindow : UISimpleWindow
    {
        public GameObject ItemSlotPrefab => _itemSlotPrefab;
        public GameObject EmptySlotPrefab => _emptySlotPrefab;
        public Transform ReserveContent => _reserveContent;
        public Transform RepairContent => _repairContent;
        
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _reseveHeaderText;
        [SerializeField] private TextMeshProUGUI _repairHeaderText;
        [SerializeField] private TextMeshProUGUI _infoButtonText;

        [Header("Settings")]
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button _createRandom;
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private RectTransform _reserveContent;
        [SerializeField] private RectTransform _repairContent;
        [SerializeField] private GameObject _itemSlotPrefab;
        [SerializeField] private GameObject _emptySlotPrefab;
        
        private const string _headerTextKey = "UIHospital_Header";
        private const string _reseveHeaderTextKey = "UIHospital_Receve";
        private const string _repairHeaderTextKey = "UIHospital_Repair";
        private const string _infoButtonTextKey = "UIHospital_InfoButton";
        
        public event EventHandler InfoEvent;
        public event EventHandler CreateRandomEvent;
      
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _infoButton.onClick.AddListener(InfoEventHandler);
            _createRandom.onClick.AddListener(CreateRandomEventHandler);
            
            _headerText.text = LocalizationManager.Localize(_headerTextKey);
            _reseveHeaderText.text = LocalizationManager.Localize(_reseveHeaderTextKey);
            _repairHeaderText.text = LocalizationManager.Localize(_repairHeaderTextKey);
            _infoButtonText.text = LocalizationManager.Localize(_infoButtonTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _infoButton.onClick.RemoveListener(InfoEventHandler);
            _createRandom.onClick.RemoveListener(CreateRandomEventHandler);

            InfoEvent = null;
            CreateRandomEvent = null;
        }

        private void InfoEventHandler()
        {
            InfoEvent?.Invoke(this, EventArgs.Empty);
        }
        private void CreateRandomEventHandler()
        {
            CreateRandomEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
