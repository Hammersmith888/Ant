using BugsFarm.Services.BootstrapService;
using UnityEngine;

namespace BugsFarm.SimulationSystem
{
    public class SimulationCommand : Command
    {
        private readonly ISimulationSystem _simulationSystem;

        public SimulationCommand(ISimulationSystem simulationSystem)
        {
            _simulationSystem = simulationSystem;
        }

        public override void Do()
        {
            _simulationSystem.OnSimulationEnd += OnSimulationEnded;
            var simulateTime = Tools.UtcNow() - _simulationSystem.LastExitTime;
            if (simulateTime <= 0)
            {
                Debug.LogError($"{this} : Cant simulate zero seconds");
                OnSimulationEnded();
                return;
            }
            _simulationSystem.Simulate(simulateTime);
        }

        private void OnSimulationEnded()
        {
            _simulationSystem.OnSimulationEnd -= OnSimulationEnded;
            OnDone();
        }
    }
}