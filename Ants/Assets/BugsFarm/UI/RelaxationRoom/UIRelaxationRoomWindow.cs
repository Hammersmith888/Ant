using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIRelaxationRoomWindow : UISimpleWindow
    {
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _descriptionText;
        [SerializeField] private TextMeshProUGUI _playText;
        [SerializeField] private TextMeshProUGUI _infoButtonText;
        [SerializeField] private TextMeshProUGUI _crystallsText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        
        [SerializeField] private GameObject _crystallsContainer;
        [SerializeField] private GameObject _coinsContainer;
        
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private Button _buttonPlay;
        [SerializeField] private Button _buttonMove;
        [SerializeField] private Button _buttonInfo;
        
        private const string _headerTextKey = "UIRelaxationRoom_Header";
        private const string _descriptionTextKey = "UIRelaxationRoom_Discription";
        private const string _playTextKey = "UIRelaxationRoom_Play";
        private const string _infoButtonTextKey = "UIRelaxationRoom_Info"; 
        
        public event EventHandler PlayAdsEvent;
        public event EventHandler MoveEvent;
        public event EventHandler InfoEvent;
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _buttonPlay.onClick.AddListener(PlayAdsEventHandler);
            _buttonMove.onClick.AddListener(MoveEventHandler);
            _buttonInfo.onClick.AddListener(InfoEventHandler);
            
            _headerText.text = LocalizationManager.Localize(_headerTextKey);
            _descriptionText.text = LocalizationManager.Localize(_descriptionTextKey);
            _playText.text = LocalizationManager.Localize(_playTextKey);
            _infoButtonText.text = LocalizationManager.Localize(_infoButtonTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _buttonPlay.onClick.RemoveListener(PlayAdsEventHandler);
            _buttonMove.onClick.RemoveListener(MoveEventHandler);
            _buttonInfo.onClick.RemoveListener(InfoEventHandler);

            PlayAdsEvent = null;
            MoveEvent = null;
            InfoEvent = null;
        }
               
        public void SetCoinsCount(string value)
        {
            _coinsText.text = value;
        }
       
        public void SetCrystalsCount(string value)
        {
            _crystallsText.text = value;
        }

        private void PlayAdsEventHandler()
        {
            PlayAdsEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MoveEventHandler()
        {
            MoveEvent?.Invoke(this, EventArgs.Empty);
        }

        private void InfoEventHandler()
        {
            InfoEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
