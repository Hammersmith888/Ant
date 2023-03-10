using System;
using System.Collections.Generic;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UIQuestsWindow : UISimpleWindow
    {
        public List<UIQuestGiftPoint> GiftPoints => giftPoints;
        public UIQuestItem QuestItemPrefab => _prefab;
        public Transform Content => _content;

        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _dayText;
        [SerializeField] private TextMeshProUGUI _progressText;

        [SerializeField] private List<UIQuestGiftPoint> giftPoints;
        
        [SerializeField] private Image _progressFill;
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private Transform _content;
        [SerializeField] private UIQuestItem _prefab;
        
        private const string _headerTextKey = "UIQuests_Header";
        
        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            
            _headerText.text = LocalizationManager.Localize(_headerTextKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }
        }
        
        public void SetDay(int day)
        {
            _dayText.text = day.ToString();
        }
        public void ChangeProgressFill(float fillAmount01)
        {
            if (!_progressFill)
            {
                Debug.LogError($" {nameof(ChangeProgressFill)} :: {nameof(_progressFill)} is Null", this);
                return;
            }

            var percentProgress = (int) (fillAmount01 * 100.0f);

            _progressText.text = percentProgress.ToString() + "%";
            _progressFill.fillAmount = fillAmount01;

        }

    }
}