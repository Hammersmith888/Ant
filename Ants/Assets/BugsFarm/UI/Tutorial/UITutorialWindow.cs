using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UITutorialWindow : UISimpleWindow
    {
        [SerializeField] private SkeletonGraphic _spineAnimation;
        [SerializeField] private TextMeshProUGUI _skipButtonText;
        [SerializeField] private TextMeshProUGUI _messageText;
        [SerializeField] private Button _skipButton;
        [SerializeField] private Button _nextButton;

        private const string _skipButtonTextKey = "UITutorial_Skip";
        
        public event EventHandler SkipEvent;
        public event EventHandler NextEvent;
        
        public SkeletonGraphic SpineAnimation => _spineAnimation;

        public override void Show()
        {
            base.Show();
            _skipButton.onClick.AddListener(OnSkipEventHandler);
            _nextButton.onClick.AddListener(OnNextEventHandler);
            _skipButtonText.text = LocalizationManager.Localize(_skipButtonTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            _skipButton.onClick.RemoveListener(OnSkipEventHandler);
            _nextButton.onClick.RemoveListener(OnNextEventHandler);
            SkipEvent = null;
            NextEvent = null;
        }

        public void SetMessageText(string text)
        {
            _messageText.text = text;
        }

        public void SetNextButtonActive(bool value)
        {
            _nextButton.gameObject.SetActive(value);
        }

        public void SetSkipButtonActive(bool value)
        {
            _skipButton.gameObject.SetActive(value);
        }

        private void OnSkipEventHandler()
        {
            SkipEvent?.Invoke(this, EventArgs.Empty);
        }

        private void OnNextEventHandler()
        {
            NextEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
