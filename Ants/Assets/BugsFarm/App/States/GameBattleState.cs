using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.StateMachine;

namespace BugsFarm.App.States
{
    public class GameBattleState : State
    {
        private readonly InteractorStorage _interactorStorage;

        public GameBattleState(InteractorStorage interactorStorage) : base("Battle")
        {
            _interactorStorage = interactorStorage;
        }
        
        public override void OnEnter(params object[] args)
        {
            base.OnEnter(args);
        }

        public override void OnExit()
        {
            base.OnExit();
        }
    }
}