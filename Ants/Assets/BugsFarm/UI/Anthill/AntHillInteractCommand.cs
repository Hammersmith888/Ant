using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StateMachine;
using Zenject;

namespace BugsFarm.UI
{
    public class AntHillInteractCommand : InteractionBaseCommand
    {
        private readonly StateMachine _stateMachine;
        private readonly IInstantiator _instantiator;
        private AntHillInteractionCommand _mainInteractor;
        private UIAntHillTaskInteractionCommand _taskInteractor;
        private readonly AntHillTaskHandler _antHillTaskHandler;

        private const string _mainInteractorID = "AntHill";
        private const string _taskInteractorID = "AntHillTask";
        
        public AntHillInteractCommand(IInstantiator instantiator, AntHillTaskHandler antHillTaskHandler)
        {
            _antHillTaskHandler = antHillTaskHandler;
            _instantiator = instantiator;
            _stateMachine = new StateMachine();
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            _mainInteractor = _instantiator.Instantiate<AntHillInteractionCommand>();
            _mainInteractor.SetDependencies(protocol);
            _mainInteractor.OnCloseWindow += CloseWindow;
            _mainInteractor.OnTaskListPressed += SwitchToTask;
            _stateMachine.Add(_mainInteractor);

            _taskInteractor = _instantiator.Instantiate<UIAntHillTaskInteractionCommand>();
            _taskInteractor.OnCloseWindow += SwitchToMainInteractor;
            _stateMachine.Add(_taskInteractor);

            _antHillTaskHandler.UpdateAll();
            
            _stateMachine.Switch(_mainInteractorID);
            
            return Task.CompletedTask;
        }

        private void SwitchToMainInteractor()
        {
            _stateMachine.Switch(_mainInteractorID);
        }

        private void SwitchToTask()
        {
            _stateMachine.Switch(_taskInteractorID);
        }

        private void CloseWindow()
        {
            _mainInteractor.OnCloseWindow -= CloseWindow;
            _mainInteractor.OnTaskListPressed -= SwitchToTask;
            _taskInteractor.OnCloseWindow -= SwitchToMainInteractor;
            _stateMachine.Exit(_mainInteractorID);
        }
    }
}