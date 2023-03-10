using BugsFarm.SimulationSystem;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class SimulatedTimerTask : BaseTimerTask
    {
        private ISimulationSystem _simulationSystem;
        private ITickableManager _tickableManager;
        
        [Inject]
        private void Inject(ITickableManager tickableManager, 
                            ISimulationSystem simulationSystem)
        {
            _tickableManager = tickableManager;
            _simulationSystem = simulationSystem;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            _tickableManager.Add(this);
        }
        
        public override void Tick()
        {
            if (!IsRunned)
            {
                return;
            }
            
            ApplyTime(_simulationSystem.DeltaTime);
        }


        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }
        }
    }
}