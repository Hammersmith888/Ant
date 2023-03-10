using BugsFarm.BuildingSystem;
using BugsFarm.UI;
using Zenject;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class GoldmineTaskAssigner : ITaskAssigner
    {
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly GoldmineAssignableTaskProcessor _goldmineProcessor;
        private readonly SimulatingTeleporter _simulatingTeleporter;

        
        public GoldmineTaskAssigner(IInstantiator instantiator, BuildingDtoStorage buildingDtoStorage, SimulatingTeleporter simulatingTeleporter)
        {
            _simulatingTeleporter = simulatingTeleporter;
            _goldmineProcessor = instantiator.Instantiate<GoldmineAssignableTaskProcessor>();
            _buildingDtoStorage = buildingDtoStorage;
        }

        public bool CanAssign(string guid)
        {
            return _goldmineProcessor.CanExecute(guid);
        }

        public void Assign(string guid)
        {
            var info = _goldmineProcessor.TaskInfo;
            _goldmineProcessor.Execute(guid);
            _simulatingTeleporter.TeleportToConcrete(guid, info.OwnerId);
        }
    }
}