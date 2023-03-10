using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using TMPro;
using UniRx;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI
{
    public class UISafeWindow : UISimpleWindow
    {
        public event EventHandler BuyEvent;
        public event EventHandler InfoEvent;
        public Vector2 CurrencyTarget => _fullState.transform.position;

        [Header("Texts")] 
        [SerializeField] private TextMeshProUGUI _headerText;
        [SerializeField] private TextMeshProUGUI _fromText;
        [SerializeField] private TextMeshProUGUI _toText;
        [SerializeField] private TextMeshProUGUI _fullStateCountText;
        [SerializeField] private TextMeshProUGUI _timerText;
        [SerializeField] private TextMeshProUGUI _buyText;
        [SerializeField] private TextMeshProUGUI _priceText;
        [SerializeField] private TextMeshProUGUI _progressText;

        [Header("Buttons")] 
        [SerializeField] private Button _buyButton;
        [SerializeField] private Button _infoButton;
        [SerializeField] private Button[] _closeButtons;

        [Header("States")] 
        [SerializeField] private GameObject _emptyState;
        [SerializeField] private GameObject _fullState;
        [SerializeField] private GameObject _timerContainer;
        [SerializeField] private Image _progressFill;

        private const string _headerTextKey = "UISafe_Header";
        private const string _priceTextKey = "UISafe_Price";

        public override void Show()
        {
            base.Show();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }

            _buyButton.onClick.AddListener(BuyEventHandler);
            _infoButton.onClick.AddListener(InfoEventHandler);

            _headerText.text = LocalizationManager.Localize(_headerTextKey);
            _buyText.text = LocalizationManager.Localize(_priceTextKey);
            
            _boss.onClick.AddListener(OnBoss);
            _season.onClick.AddListener(OnSeason);
            _level.onClick.AddListener(OnLevel);
            _quest.onClick.AddListener(OnQuest);
            _arena.onClick.AddListener(OnArena);
        }

        public override void Hide()
        {
            base.Hide();

            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.RemoveListener(Close);
            }

            _buyButton.onClick.RemoveListener(BuyEventHandler);
            _infoButton.onClick.RemoveListener(InfoEventHandler);
            
            _boss.onClick.RemoveListener(OnBoss);
            _season.onClick.RemoveListener(OnSeason);
            _level.onClick.RemoveListener(OnLevel);
            _quest.onClick.RemoveListener(OnQuest);
            _arena.onClick.RemoveListener(OnArena);

            BuyEvent = null;
            InfoEvent = null;
        }

        public void SetPriceText(string value)
        {
            _priceText.text = value;
        }

        public void SetProgressText(string value)
        {
            _progressText.text = value;
        }

        public void SetCountMaxText(string value)
        {
            _toText.text = value;
            _fullStateCountText.text = value;
        }

        public void SetTimerText(string value)
        {
            _timerText.text = value;
        }

        public void SetProgress(float progress01)
        {
            _progressFill.fillAmount = progress01;
        }
        
        public void SetProgressActive(bool value)
        {
            _progressFill.transform.parent.gameObject.SetActive(value);
        }
        
        public void SetBuyButtonActive(bool value)
        {
            _buyButton.gameObject.SetActive(value);
        }

        public void SetTimerActive(bool value)
        {
            _timerContainer.SetActive(value);
        }
        
        public void SetStateActive(bool full)
        {
            _emptyState.SetActive(!full);
            _fullState.SetActive(full);
        }

        private void BuyEventHandler()
        {
            BuyEvent?.Invoke(this, EventArgs.Empty);
        }

        private void InfoEventHandler()
        {
            InfoEvent?.Invoke(this, EventArgs.Empty);
        }

    #region TMP

        [SerializeField] private Button _boss;
        [SerializeField] private Button _season;
        [SerializeField] private Button _level;
        [SerializeField] private Button _quest;
        [SerializeField] private Button _arena;
        
        private void OnBoss()
        {
            MessageBroker.Default.Publish(new AchievementProtocol{Id = Achievement.FightedBoss});
        }
        private void OnSeason()
        {
            MessageBroker.Default.Publish(new AchievementProtocol{Id = Achievement.NewSeason});
        }
        private void OnLevel()
        {
            MessageBroker.Default.Publish(new AchievementProtocol{Id = Achievement.LevelUp});
        }
        private void OnQuest()
        {
            MessageBroker.Default.Publish(new AchievementProtocol{Id = Achievement.QuestDone});
        }
        private void OnArena()
        {
            MessageBroker.Default.Publish(new AchievementProtocol{Id = Achievement.ArenaWin});
        }

    #endregion
    }
}