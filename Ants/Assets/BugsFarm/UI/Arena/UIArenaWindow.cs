using System;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace BugsFarm.UI.Arena
{
    public class UIArenaWindow : UISimpleWindow
    {
        [SerializeField] private Button[] _closeButtons;
        [SerializeField] private Button _rankingButton;
        [SerializeField] private Button _changeButton;
        [SerializeField] private Button _fightButton;
        [SerializeField] private TextMeshProUGUI _headerLabel;
        [SerializeField] private TextMeshProUGUI _yourTeamLabel;
        [SerializeField] private TextMeshProUGUI _changeButtonLabel;
        [SerializeField] private TextMeshProUGUI _fightButtonLabel;
        
        private const string _headerLabelKey = "UIArena_Header";
        private const string _yourTeamLabelKey = "UIArena_YourTeam";
        private const string _changeButtonLabelKey = "UIArena_ChangeButton";
        private const string _fightButtonLabelKey = "UIArena_FightButton";

        public event EventHandler RankingEvent;
        public event EventHandler ChangeEvent; 
        public event EventHandler FightEvent;

        public override void Show()
        {
            base.Show();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _rankingButton.onClick.AddListener(RankingEventHandler);
            _changeButton.onClick.AddListener(ChangeEventHandler);
            _fightButton.onClick.AddListener(FightEventHandler);
            
            _headerLabel.text = LocalizationManager.Localize(_headerLabelKey);
            _yourTeamLabel.text = LocalizationManager.Localize(_yourTeamLabelKey);
            _changeButtonLabel.text = LocalizationManager.Localize(_changeButtonLabelKey);
            _fightButtonLabel.text = LocalizationManager.Localize(_fightButtonLabelKey);
        }

        public override void Hide()
        {
            base.Hide();
            
            foreach (var closeButton in _closeButtons)
            {
                closeButton.onClick.AddListener(Close);
            }
            _rankingButton.onClick.RemoveListener(RankingEventHandler);
            _changeButton.onClick.RemoveListener(ChangeEventHandler);
            _fightButton.onClick.RemoveListener(FightEventHandler);
            
            RankingEvent = null;
            ChangeEvent = null; 
            FightEvent = null;
        }
        
        private void RankingEventHandler()
        {
            RankingEvent?.Invoke(this, EventArgs.Empty);
        }

        private void ChangeEventHandler()
        {
            ChangeEvent?.Invoke(this, EventArgs.Empty);
        }

        private void FightEventHandler()
        {
            FightEvent?.Invoke(this, EventArgs.Empty);
        }
    }
}
