using BugsFarm.BuildingSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class DigTaskAssigner : ITaskAssigner
    {
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private readonly DigAssignableTaskProcessor _digProcessor;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;

        public DigTaskAssigner(IInstantiator instantiator, SimulatingTeleporter simulatingTeleporter, UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _simulatingTeleporter = simulatingTeleporter;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _digProcessor = instantiator.Instantiate<DigAssignableTaskProcessor>();
        }

        public bool CanAssign(string guid)
        {
            return _digProcessor.CanExecute(guid);
        }

        public void Assign(string guid)
        {
            var taskProcessor = _unitTaskProcessorStorage.Get(guid);
            if (taskProcessor.GetCurrentTask().GetName() == nameof(GoldmineBootstrapTask))
            {
                _simulatingTeleporter.TeleportToAny(guid, "44");
                return;
            }
            
            var info = _digProcessor.TaskInfo;
            _simulatingTeleporter.TeleportToConcreteRoom(guid, info.OwnerId);
            _digProcessor.Execute(guid);
        }
    }
}