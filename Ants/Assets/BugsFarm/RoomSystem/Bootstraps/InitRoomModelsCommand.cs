using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class InitRoomModelsCommand : Command
    {
        public InitRoomModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<RoomModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<RoomNeighbourModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<RoomStatModel>>());
        }
    }
}