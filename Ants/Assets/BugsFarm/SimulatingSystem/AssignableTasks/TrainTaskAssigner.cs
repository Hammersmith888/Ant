using BugsFarm.UI;
using Zenject;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class TrainTaskAssigner : ITaskAssigner
    {
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private readonly TrainAssignableTaskProcessor _trainProcessor;

        public TrainTaskAssigner(IInstantiator instantiator, SimulatingTeleporter simulatingTeleporter)
        {
            _simulatingTeleporter = simulatingTeleporter;
            _trainProcessor = instantiator.Instantiate<TrainAssignableTaskProcessor>();
        }

        public bool CanAssign(string guid)
        {
            return _trainProcessor.CanExecute(guid);
        }

        public void Assign(string guid)
        {
            var info = _trainProcessor.TaskInfo;
            _trainProcessor.Execute(guid);
            _simulatingTeleporter.TeleportToConcrete(guid, info.OwnerId);
        }
    }
}