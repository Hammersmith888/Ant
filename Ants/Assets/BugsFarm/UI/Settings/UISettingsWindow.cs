using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISettingsWindow : UISimpleWindow
    {
        [Header("Texts")]
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _languageText;
        [SerializeField] private TextMeshProUGUI _resetText;
        [SerializeField] private TextMeshProUGUI _userAgreementsText;
        [SerializeField] private TextMeshProUGUI _privacyPolicyText;
    
        [Header("Buttons")]
        [SerializeField] private Button _languageButton;
        [SerializeField] private Button _resetButton;
        [SerializeField] private Button _userAgrementsButton;
        [SerializeField] private Button _privacyPolicyButton;
        [SerializeField] private Button _soundButton;
        [SerializeField] private Button _musicButton;
        [SerializeField] private Button[] _closeButtons;
        
        [SerializeField] private Image _soundButtonIcon;
        [SerializeField] private Image _musicButtonIcon;
     
        private const string _headerTextKey = "UISettingsWindow_Header";
        private const string _resetTextKey = "UISettingsWindow_Reset";
        private const string _userAgreementsTextKey = "UISettingsWindow_UserAgreements";
        private const string _privacyPolicyTextKey = "UISettingsWindow_PrivacyPolicy";
        
        public event EventHandler LanguageEvent;
        public event EventHandler ResetEvent;
        public event EventHandler UserAgreementsEvent;
        public event EventHandler PrivacyPolicyEvent;
        public event EventHandler SoundEvent;
        public event EventHandler MusicEvent;

        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _languageButton.onClick.AddListener(LanguageEventHandler);
            _resetButton.onClick.AddListener(ResetEventHandler);
            _userAgrementsButton.onClick.AddListener(UserAgreementsEventHandler);
            _privacyPolicyButton.onClick.AddListener(PrivacyPolicyEventHandler);
            _soundButton.onClick.AddListener(SoundEventHandler);
            _musicButton.onClick.AddListener(MusicEventHandler);
            
            _headerText.text = LocalizationManager.Localize(_headerTextKey);
            _resetText.text = LocalizationManager.Localize(_resetTextKey);
            _userAgreementsText.text = LocalizationManager.Localize(_userAgreementsTextKey);
            _privacyPolicyText.text = LocalizationManager.Localize(_privacyPolicyTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
            _languageButton.onClick.RemoveListener(LanguageEventHandler);
            _resetButton.onClick.RemoveListener(ResetEventHandler);
            _userAgrementsButton.onClick.RemoveListener(UserAgreementsEventHandler);
            _privacyPolicyButton.onClick.RemoveListener(PrivacyPolicyEventHandler);
            _soundButton.onClick.RemoveListener(SoundEventHandler);
            _musicButton.onClick.RemoveListener(MusicEventHandler);

            LanguageEvent = null;
            ResetEvent = null;
            UserAgreementsEvent = null;
            PrivacyPolicyEvent = null;
            SoundEvent = null;
            MusicEvent = null;
        }
                
        public void SetLanguageText(string text)
        {
            _languageText.text = text;
        }
                
        public void SetSoundIcon(Sprite icon)
        {
            _soundButtonIcon.sprite = icon;
        }
                
        public void SetMusicIcon(Sprite icon)
        {
            _musicButtonIcon.sprite = icon;
        }

        private void LanguageEventHandler()
        {
            LanguageEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ResetEventHandler()
        {
            ResetEvent?.Invoke(this, EventArgs.Empty);
        }

        private void UserAgreementsEventHandler()
        {
            UserAgreementsEvent?.Invoke(this, EventArgs.Empty);
        }

        private void PrivacyPolicyEventHandler()
        {
            PrivacyPolicyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void SoundEventHandler()
        {
            SoundEvent?.Invoke(this, EventArgs.Empty);
        }

        private void MusicEventHandler()
        {
            MusicEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
