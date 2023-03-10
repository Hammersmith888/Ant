using BugsFarm.App;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using Ecs.Controllers;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class BattleState : State
    {
        private readonly IStateMachine _stateMachine;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly BattleEcsController _battleEcsController;

        public BattleState(
            IStateMachine stateMachine, 
            IUIService uiService,
            IInstantiator instantiator) : base("Battle")
        {
            _stateMachine = stateMachine;
            _uiService = uiService;
            _instantiator = instantiator;
        }

        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);

            _instantiator.Instantiate<LoadSceneCommand>(new object[] {"GlobalMap"}).Do();
            _uiService.Hide<UIBottomWindow>();
            _uiService.Hide<UIHeaderWindow>();
            MessageBroker.Default.Publish<GameStateChangeRequest>(new GameStateChangeRequest("Battle"));
        }

        public override void OnExit()
        {
            base.OnExit();
            
            _uiService.Show<UIBottomWindow>();
            _uiService.Show<UIHeaderWindow>();
        }
    }
}