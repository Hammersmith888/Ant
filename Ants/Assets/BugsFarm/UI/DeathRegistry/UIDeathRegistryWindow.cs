using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIDeathRegistryWindow : UISimpleWindow
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _headerNameText;    
        [SerializeField] private TextMeshProUGUI _deathText;    
        [SerializeField] private TextMeshProUGUI _deathReasonText;    
        [SerializeField] private TextMeshProUGUI _restText;    
        [SerializeField] private TextMeshProUGUI _deathTimeText;    
        [SerializeField] private TextMeshProUGUI _descriptionText; 
        
        [Header("Components")]
        [SerializeField] private Image _avatarImage;    
        [SerializeField] private Button _deleteButton;    
        [SerializeField] private Button[] _closeButtons;
        
        private const string _deathTextKey = "UIDeathRegistry_Death";
        private const string _restTextKey = "UIDeathRegistry_Rest";
        
        public event EventHandler DeleteEvent;
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _deleteButton.onClick.AddListener(DeleteEventHandler);
            
            _deathText.text = LocalizationManager.Localize(_deathTextKey);
            _restText.text = LocalizationManager.Localize(_restTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _deleteButton.onClick.RemoveListener(DeleteEventHandler);

            DeleteEvent = null;
        }
               
        public void SetHeader(string header)
        {
            _headerNameText.text = header;
        }
       
        public void SetDeathReason(string deathReason)
        {
            _deathReasonText.text = deathReason;
        }
       
        public void SetDeathTime(string deathTime)
        {
            _deathTimeText.text = deathTime;
        }
     
        public void SetDescription(string description)
        {
            _descriptionText.text = description;
        }
     
        public void SetAvatarIcon(Sprite icon)
        {
            _avatarImage.sprite = icon;
        }

        private void DeleteEventHandler()
        {
            DeleteEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
