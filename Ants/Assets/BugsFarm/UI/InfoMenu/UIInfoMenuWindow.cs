using System;
using System.Collections.Generic;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIInfoMenuWindow : UISimpleWindow
    {
        [SerializeField] private Button _removeButton;//
        [SerializeField] private Button _replaceButton;//
        [SerializeField] private Button _upgradeButton;//
        [SerializeField] private Button _leftButton;//
        [SerializeField] private Button _rightButton;//
        [SerializeField] private Button _actionButton;//
        [SerializeField] private Button _restockButton;//
        [SerializeField] private Button _speedupButton;//
        [SerializeField] private Button[] _closeButtons;
    
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;//
        [SerializeField] private TextMeshProUGUI _typeLabel;//
        [SerializeField] private TextMeshProUGUI _statLabel;//
        [SerializeField] private TextMeshProUGUI _upgradeCostLabel;
        [SerializeField] private TextMeshProUGUI _upgradeButtonLabel;
        [SerializeField] private TextMeshProUGUI _actionButtonLabel;
        [SerializeField] private TextMeshProUGUI _restockCostLabel;
        [SerializeField] private TextMeshProUGUI _restockButtonLabel;
        [SerializeField] private TextMeshProUGUI _speedupCostLabel;
        [SerializeField] private TextMeshProUGUI _speedupButtonLabel;
        [SerializeField] private TextMeshProUGUI _resourceInfoLabel;
        [SerializeField] private TextMeshProUGUI _buildingProgressLabel;
    
        [SerializeField] private Image _resourceIconImage;
        [SerializeField] private Image _resourceCurrencyIconImage;
        [SerializeField] private SlicedFilledImage _resourceFillProgress;  
        [SerializeField] private SlicedFilledImage _buildingFillProgress; 
        [SerializeField] private GameObject _resourceBarContainer;        
        [SerializeField] private GameObject _buildingProgressContainer;

        [SerializeField] private UIInfoParamCell _paramCellPrefab;
        [SerializeField] private Transform _paramCellContainer;
        
        private readonly List<UIInfoParamCell> _paramCells = new List<UIInfoParamCell>();
        
        private const string _upgradeButtonLabelKey = "UIInfoMenu_UpgradeButton";
        private const string _actionButtonLabelKey = "UIInfoMenu_ActionButton";
        private const string _restockButtonLabelKey = "UIInfoMenu_RestockButton";
        private const string _speedupButtonLabelKey = "UIInfoMenu_SpeedupButton";

        #region Events

        public event EventHandler RemoveEvent;
        public event EventHandler ReplaceEvent;
        public event EventHandler UpgradeEvent; 
        public event EventHandler ActionEvent;
        public event EventHandler ArrowLeftEvent;
        public event EventHandler ArrowRightEvent;
        public event EventHandler RestockEvent; 
        public event EventHandler SpeedupEvent; 

        #endregion
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _removeButton.onClick.AddListener(RemoveEventHandler);
            _replaceButton.onClick.AddListener(ReplaceEventHandler);
            _upgradeButton.onClick.AddListener(UpgradeEventHandler);
            _leftButton.onClick.AddListener(ArrowLeftEventHandler);
            _rightButton.onClick.AddListener(ArrowRightEventHandler);
            _actionButton.onClick.AddListener(ActionEventHandler);
            _restockButton.onClick.AddListener(RestockEventHandler);
            _speedupButton.onClick.AddListener(SpeedupEventHandler);
            
            _upgradeButtonLabel.text = LocalizationManager.Localize(_upgradeButtonLabelKey);
            _actionButtonLabel.text = LocalizationManager.Localize(_actionButtonLabelKey);
            _restockButtonLabel.text = LocalizationManager.Localize(_restockButtonLabelKey);
            _speedupButtonLabel.text = LocalizationManager.Localize(_speedupButtonLabelKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _removeButton.onClick.RemoveListener(RemoveEventHandler);
            _replaceButton.onClick.RemoveListener(ReplaceEventHandler);
            _upgradeButton.onClick.RemoveListener(UpgradeEventHandler);
            _leftButton.onClick.RemoveListener(ArrowLeftEventHandler);
            _rightButton.onClick.RemoveListener(ArrowRightEventHandler);
            _actionButton.onClick.RemoveListener(ActionEventHandler);
            _restockButton.onClick.RemoveListener(RestockEventHandler);
            _speedupButton.onClick.RemoveListener(SpeedupEventHandler);
            ClearParams();
            
            RemoveEvent = null;
            ReplaceEvent = null;
            UpgradeEvent = null;
            ActionEvent = null;
            ArrowLeftEvent = null;
            ArrowRightEvent = null;
            RestockEvent = null;
            SpeedupEvent = null;
        }

        public void SetHeader(string header)
        {
            _headerLabel.text = header;
        }

        public void SetDescription(string text)
        {
            _descriptionLabel.text = text;
        }
        
        public void SetTypeName(string typeName)
        {
            _typeLabel.text = typeName;
        }

        public void SetStat(string value)
        {
            _statLabel.text = value;
        }
  
        public void SetIcon(Sprite iconSprite)
        {
            _resourceIconImage.sprite = iconSprite;
        }

        public void SetBuildingProgress(float value)
        {
            _buildingFillProgress.fillAmount = value;
        } 
        
        public void SetProgressCount(string value)
        {
            _buildingProgressLabel.text = value;
        }

        #region ResourceMethods

        public void SetResourceInfo(string resourceInfo)
        {
            _resourceInfoLabel.text = resourceInfo;
        } 
        
        public void SetResourceBarProgress(float value)
        {
            _resourceFillProgress.fillAmount = value;
        } 
                
        public void SetResourceCurrencyIcon(Sprite value)
        {
            _resourceCurrencyIconImage.sprite = value;
        }
            
        public void SetResourceCurrencyIconActive(bool value)
        {
            _resourceCurrencyIconImage.gameObject.SetActive(value);
        }
        
        public void SetResourceActive(bool value)
        {
            _resourceInfoLabel.gameObject.SetActive(value);
        }

        #endregion

        #region UpgradeButton

        public void SetUpgradeButtonActive(bool value)
        {
            _upgradeButton.gameObject.SetActive(value);
        }
        
        public void SetUpgradeButtonInteractable(bool value)
        {
            _upgradeButton.interactable  = value;
        }

        public void SetUpgradeCost(string value)
        {
            _upgradeCostLabel.text = value;
        }

        #endregion

        #region RestockButton

        public void SetRestockButtonActive(bool value)
        {
            _restockButton.gameObject.SetActive(value);
        }
        
        public void SetRestockButtonInteractable(bool value)
        {
            _restockButton.interactable  = value;
        }
        
        public void SetRestockCost(string value)
        {
            _restockCostLabel.text = value;
        }

        #endregion

        #region SpeedupButton

        public void SetSpeedupButtonActive(bool value)
        {
            _speedupButton.gameObject.SetActive(value);
        }
        
        public void SetSpeedupButtonInteractable(bool value)
        {
            _speedupButton.interactable  = value;
        }

        public void SetSpeedupCost(string value)
        {
            _speedupCostLabel.text = value;
        }

        #endregion

        #region SetElementsActive
        public void SetRemoveButtonActive(bool value)
        {
            _removeButton.gameObject.SetActive(value);
        }
        
        public void SetMoveButtonActive(bool value)
        {
            _replaceButton.gameObject.SetActive(value);
        }
    
        public void SetResourceBarActive(bool value) 
        {
            _resourceBarContainer.SetActive(value);
        }                
        
        public void SetBuildingBarActive(bool value) 
        {
            _buildingProgressContainer.SetActive(value);
        }  
        
        public void SetLeftButtonActive(bool value) 
        {
            _leftButton.gameObject.SetActive(value);
        }  
        
        public void SetRightButtonActive(bool value) 
        {
            _rightButton.gameObject.SetActive(value);
        }   
        
        public void SetActionButtonActive(bool value) 
        {
            _actionButton.gameObject.SetActive(value);
        }   
        
        public void SetDescriptionTextActive(bool value) 
        {
            _descriptionLabel.gameObject.SetActive(value);
        }   
        
        public void SetTypeLabelActive(bool value) 
        {
            _typeLabel.gameObject.SetActive(value);
        }   
        
        public void SetStatLabelActive(bool value) 
        {
            _statLabel.gameObject.SetActive(value);
        }   
        #endregion
     
        public void SetParams(IEnumerable<InfoParamData> paramValues)
        {
            ClearParams();
            if (paramValues == null)
            {
                return;
            }
            
            foreach (var data in paramValues)
            {
                var cell = Instantiate(_paramCellPrefab, _paramCellContainer);
                cell.SetName(data.Name + " : ");
                cell.SetValue(Format.FormatId(data.FormatId, data.Value));

                if (data.UpgradeValue >= 0)
                {
                    var diff = data.UpgradeValue - data.Value;
                    var sign = Mathf.Sign(diff) > 0 ? "+" : "-";
                    diff = Mathf.Abs(diff);
                    cell.SetUpgradeValue($"{sign} {Format.FormatId(data.FormatId, diff)}");
                    cell.SetUpgradeActive(true);
                }
                else
                {
                    cell.SetUpgradeActive(false);
                }
                
                cell.SetIconActive(data.Icon);
                cell.SetIcon(data.Icon);
                _paramCells.Add(cell);
            }
        }
          
        private void ClearParams()
        {
            foreach (var paramCell in _paramCells)
            {
                Destroy(paramCell.gameObject);
            }

            _paramCells.Clear();
        }

        #region EventHandlers

        private void RemoveEventHandler()
        {
            RemoveEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void ReplaceEventHandler()
        {
            ReplaceEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void UpgradeEventHandler()
        {
            UpgradeEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void ActionEventHandler()
        {
            ActionEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ArrowLeftEventHandler()
        {
            ArrowLeftEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void ArrowRightEventHandler()
        {
            ArrowRightEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void RestockEventHandler()
        {
            RestockEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void SpeedupEventHandler()
        {
            SpeedupEvent?.Invoke(this, EventArgs.Empty);
        }

        #endregion
    }
}
