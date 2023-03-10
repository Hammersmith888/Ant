using BugsFarm.Services.InputService;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using Zenject;

namespace BugsFarm.UI
{
    public class MyBugsState : State
    {
        private readonly IStateMachine _bottomStateMachine;
        private readonly IStateMachine _terrariumStateMachine;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IUIService _uiService;
        private string _lastTabSelected = "Catalog";

        public MyBugsState(IStateMachine bottomStateMachine,
                           IStateMachine terrariumStateMachine,
                           IInputController<MainLayer> inputController,
                           IInstantiator instantiator,
                           IUIService uiService) : base("MyBugs")
        {
            _bottomStateMachine = bottomStateMachine;
            _terrariumStateMachine = terrariumStateMachine;
            _inputController = inputController;
            _uiService = uiService;
            
            var args = new object[] {_terrariumStateMachine};
            _terrariumStateMachine.Add(instantiator.Instantiate<CatalogState>()); 
            _terrariumStateMachine.Add(instantiator.Instantiate<TerrariumState>(args));        
            _terrariumStateMachine.Add(instantiator.Instantiate<WikiState>(args));       
        }

        public override void OnEnter(params object[] args)
        {
            _inputController.Lock();
            var window = _uiService.Show<UIMyBugsWindow>();
            SwitchTab(this, _lastTabSelected);
            window.CloseEvent += (sender, eventArgs) =>
            {
                _bottomStateMachine.Exit(Id);
            };
            window.TabEvent += SwitchTab;
        }

        private void SwitchTab(object sender, string tabId)
        {
            _lastTabSelected = tabId;
            var window = _uiService.Get<UIMyBugsWindow>();
            window.SetPrimaryTab(tabId);
            _terrariumStateMachine.Switch(tabId);
        }

        public override void OnExit()
        {
            _uiService.Hide<UIMyBugsWindow>();
            _terrariumStateMachine.Switch("Empty");
            _inputController.UnLock();
        }
    }
}