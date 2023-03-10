using BugsFarm.UI;
using Zenject;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class PatrolTaskAssigner : ITaskAssigner
    {
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private readonly PatrolAssignableTaskProcessor _trainProcessor;

        public PatrolTaskAssigner(IInstantiator instantiator, SimulatingTeleporter simulatingTeleporter)
        {
            _simulatingTeleporter = simulatingTeleporter;
            _trainProcessor = instantiator.Instantiate<PatrolAssignableTaskProcessor>();
        }
        
        public bool CanAssign(string guid)
        {
            return true;
        }

        public void Assign(string guid)
        {
            _simulatingTeleporter.TeleportToRandom(guid);
            _trainProcessor.Execute(guid);
        }
    }
}