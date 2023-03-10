using System;
using BugsFarm.App.States;
using BugsFarm.Logger;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.StateMachine;
using UniRx;
using Zenject;

namespace BugsFarm.App
{
    public struct GameStateChangeRequest
    {
        public readonly string State;

        public GameStateChangeRequest(string state)
        {
            State = state;
        }
    }
    
    public class GameApp
    {
        private readonly IStateMachine _stateMachine;
        private readonly IInstantiator _instantiator;
        private readonly IDisposable _gameStateChangedEvent;
        private readonly IDisposable _gameReloadingEvent;

        public GameApp(
            IStateMachine stateMachine,
            IInstantiator instantiator)
        {
            _stateMachine = stateMachine;
            _instantiator = instantiator;
            ConfigureStateMachine();

            _gameStateChangedEvent = MessageBroker.Default.Receive<GameStateChangeRequest>().Subscribe(OnGetGameStateChangeRequest);
            _gameReloadingEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloads);
        }

        private void OnGameReloads(GameReloadingReport report)
        {
            _gameReloadingEvent.Dispose();
            _gameStateChangedEvent.Dispose();
        }

        private void ConfigureStateMachine()
        {
            _stateMachine.Add(_instantiator.Instantiate<GameFarmState>());
            _stateMachine.Add(_instantiator.Instantiate<GameBattleState>());
        }

        private void OnGetGameStateChangeRequest(GameStateChangeRequest request)
        {
            _stateMachine.Switch(request.State);
        }
    }
}