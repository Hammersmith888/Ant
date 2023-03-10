using BugsFarm.Services.BootstrapService;

namespace BugsFarm.SimulatingSystem
{
    public class PostSimulatingCommand : Command
    {
        private readonly SimulatingCenter _simulatingCenter;

        public PostSimulatingCommand(SimulatingCenter simulatingCenter)
        {
            _simulatingCenter = simulatingCenter;
        }

        public override void Do()
        {
            _simulatingCenter.PostSimulate();
            OnDone();
        }
    }
}