using BugsFarm.Services.CommandService;
using UniRx;
using Task = System.Threading.Tasks.Task;

namespace BugsFarm.BuildingSystem
{
    public class SpeedUpBuildingCommand : ICommand<SpeedUpBuildingProtocol>
    {
        private readonly IBuildingBuildSystem _buildingBuildSystem;

        public SpeedUpBuildingCommand(IBuildingBuildSystem buildingBuildSystem)
        {
            _buildingBuildSystem = buildingBuildSystem;
        }

        public Task Execute(SpeedUpBuildingProtocol protocol)
        {
            _buildingBuildSystem.ForceComplete(protocol.Guid);
            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
    }
}