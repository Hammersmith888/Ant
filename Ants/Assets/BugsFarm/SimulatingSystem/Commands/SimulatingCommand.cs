using BugsFarm.Services.BootstrapService;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingCommand : Command
    {
        private readonly SimulatingCenter _simulationCenter;
        private readonly SimulatingEntityStorage _simulatingEntityStorage;

        public SimulatingCommand(SimulatingCenter simulationCenter, SimulatingEntityStorage simulatingEntityStorage)
        {
            _simulatingEntityStorage = simulatingEntityStorage;
            _simulationCenter = simulationCenter;
        }
        public override void Do()
        {
            _simulatingEntityStorage.ConstructData();
            _simulationCenter.Simulate();
            OnDone();
        }
    }
}