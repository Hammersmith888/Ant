using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using BugsFarm.Utility;

namespace BugsFarm.UI
{
    public class WikiState : State
    {
        private readonly IStateMachine _terrariumStateMachine;
        private readonly IUIService _uiService;

        private const string _buttonTextKey = "UIMyBugs_WikiAccept";
        
        public WikiState(IStateMachine terrariumStateMachine,
                         IUIService uiService) : base("Wiki")
        {
            _terrariumStateMachine = terrariumStateMachine;
            _uiService = uiService;
        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
            var modelId = (string) args[0];
            var view = _uiService.Get<UIMyBugsWindow>().WikiView;
            
            view.Show();
            view.SetHeader(LocalizationHelper.GetBugTypeName(modelId));
            view.SetDescription(LocalizationHelper.GetBugWiki(modelId));
            view.SetAcceptText(LocalizationManager.Localize(_buttonTextKey));
            view.AcceptEvent += (sender, eventArgs) =>
            {
                var backStateId = _terrariumStateMachine.Previous.Id;
                if (backStateId == "Empty" || backStateId == Id)
                {
                    _uiService.Get<UIMyBugsWindow>().Close();
                    return;
                }
                _terrariumStateMachine.Switch(backStateId);
            };
        }

        public override void OnExit()
        {
            base.OnExit();
            _uiService.Get<UIMyBugsWindow>().WikiView.Hide();
        }
    }
}