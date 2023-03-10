using System;
using System.Collections.Generic;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIBuildingInfoMenuWindow : UISimpleWindow
    {
        [SerializeField] private Button _deleteButton;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private Button _moveButton;
        [SerializeField] private Button _restockButton;
        [SerializeField] private Button _speedupButton;
        [SerializeField] private Button[] _closeButtons;

        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _descriptionLabel;
        [SerializeField] private TextMeshProUGUI _upgradeCostLabel;
        [SerializeField] private TextMeshProUGUI _upgradeButtonLabel;
        [SerializeField] private TextMeshProUGUI _buildingInfoLabel;
        [SerializeField] private TextMeshProUGUI _resourceInfoLabel;
        [SerializeField] private TextMeshProUGUI _restockCostLabel;
        [SerializeField] private TextMeshProUGUI _restockButtonLabel;
        [SerializeField] private TextMeshProUGUI _speedupCostLabel;
        [SerializeField] private TextMeshProUGUI _speedupButtonLabel;
        [SerializeField] private Image _iconImage;
        [SerializeField] private Image _resourceCurrencyIconImage; // new
        [SerializeField] private SlicedFilledImage _resourceFillProgress;  
        [SerializeField] private SlicedFilledImage _buildingFillProgress; // new
        [SerializeField] private GameObject _resourceBarContainer;        // new
        [SerializeField] private GameObject _buildingProgressContainer;                                              // new

        [SerializeField] private UIInfoParamCell _paramCellPrefab;
        [SerializeField] private Transform _paramCellContainer;
        
        // todo: move to pool
        private readonly List<UIInfoParamCell> _paramCells = new List<UIInfoParamCell>();

        public event EventHandler DeleteEvent;
        public event EventHandler MoveEvent;
        public event EventHandler UpgradeEvent; 
        public event EventHandler RestockEvent; 
        public event EventHandler SpeedupEvent; 
        
        public void SetHeader(string header)
        {
            _headerLabel.text = header;
        }

        public void SetDescription(string text)
        {
            _descriptionLabel.text = text;
        }
        
        public void SetResourceInfo(string resourceInfo) // new
        {
            _resourceInfoLabel.text = resourceInfo;
        } 
        
        public void SetResourceBarProgress(float value) // new
        {
            _resourceFillProgress.fillAmount = value;
        } 

        public void SetBuildingProgress(float value) // new
        {
            _buildingFillProgress.fillAmount = value;
        } 
        
        public void SetBuildingText(string value) // new
        {
            _buildingInfoLabel.text = value;
        } 
        
        public void SetResourceCurrencyIcon(Sprite value) // new
        {
            _resourceCurrencyIconImage.sprite = value;
        }
        
        public void SetIcon(Sprite iconSprite)
        {
            _iconImage.sprite = iconSprite;
        }

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

        public void SetUpgradeButton(bool value)
        {
            _upgradeButton.gameObject.SetActive(value);
        }
        
        public void SetUpgradeButtonInterractable(bool value)
        {
            _upgradeButton.interactable  = value;
        }

        public void SetUpgradeCost(string value)
        {
            _upgradeCostLabel.text = value;
        }
        
        public void SetUpgradeLabel(string value)
        {
            _upgradeButtonLabel.text = value;
        }

        public void SetSpeedupButton(bool value)
        {
            _speedupButton.gameObject.SetActive(value);
        }
        
        public void SetSpeedupButtonInterractable(bool value)
        {
            _speedupButton.interactable  = value;
        }

        public void SetSpeedupCost(string value)
        {
            _speedupCostLabel.text = value;
        }
        
        public void SetSpeedupLabel(string value)
        {
            _speedupButtonLabel.text = value;
        }

        public void SetDeleteButton(bool value)
        {
            _deleteButton.gameObject.SetActive(value);
        }
        
        public void SetMoveButton(bool value)
        {
            _moveButton.gameObject.SetActive(value);
        }
        
        public void SetRestockButton(bool value)
        {
            _restockButton.gameObject.SetActive(value);
        }
        
        public void SetRestockButtonInterractable(bool value)
        {
            _restockButton.interactable  = value;
        }
        
        public void SetRestockCost(string value)
        {
            _restockCostLabel.text = value;
        }
        
        public void SetRestockButtonLabel(string value)
        {
            _restockButtonLabel.text = value;
        }

        public void SetResourceBar(bool value) // new
        {
            _resourceBarContainer.SetActive(value);
        }                
        
        public void SetBuildingBar(bool value) // new
        {
            _buildingProgressContainer.SetActive(value);
        }   
        
        public void SetResourceCurrency(bool value) // new
        {
            _resourceCurrencyIconImage.gameObject.SetActive(value);
        }
        
        public void SetResourceActive(bool value) // new
        {
            _resourceInfoLabel.gameObject.SetActive(value);
        }
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _deleteButton.onClick.AddListener(OnDeleteButtonClickHandler);
            _moveButton.onClick.AddListener(OnMoveButtonClickHandler);
            _upgradeButton.onClick.AddListener(OnUpgradeButtonClickHandler);
            _restockButton.onClick.AddListener(OnRestockButtonClickHandler);
            _speedupButton.onClick.AddListener(OnSpeedupButtonClickHandler);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _deleteButton.onClick.RemoveListener(OnDeleteButtonClickHandler);
            _moveButton.onClick.RemoveListener(OnMoveButtonClickHandler);
            _upgradeButton.onClick.RemoveListener(OnUpgradeButtonClickHandler);
            _restockButton.onClick.RemoveListener(OnRestockButtonClickHandler);
            _speedupButton.onClick.RemoveListener(OnSpeedupButtonClickHandler);
            ClearParams();
        }

        public override void Close()
        {
            base.Close();

            DeleteEvent = null;
            MoveEvent = null;
            UpgradeEvent = null;
            RestockEvent = null;
            SpeedupEvent = null;
        }

        private void OnMoveButtonClickHandler()
        {
            MoveEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnUpgradeButtonClickHandler()
        {
            UpgradeEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnDeleteButtonClickHandler()
        {
            DeleteEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnRestockButtonClickHandler()
        {
            RestockEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void OnSpeedupButtonClickHandler()
        {
            SpeedupEvent?.Invoke(this, EventArgs.Empty);
        }
        
        private void ClearParams()
        {
            foreach (var paramCell in _paramCells)
            {
                Destroy(paramCell.gameObject);
            }

            _paramCells.Clear();
        }
    }
}