using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.ChestSystem;
using BugsFarm.Services.CommandService;
using BugsFarm.UI;
using Zenject;

namespace BugsFarm.Services.SceneEntity
{
    // todo: just prototype, need composite pattern
    public class InteractionCommand : ICommand<InteractionProtocol>
    {
        private readonly IInstantiator _instantiator;

        public InteractionCommand(IInstantiator instantiator)
        {
            _instantiator = instantiator;
        }
        
        public Task Execute(InteractionProtocol protocol)
        {
            switch (protocol.ObjectType)
            {
                case SceneObjectType.Unit:        return Create<AntInteractionCommand>(protocol);
                case SceneObjectType.Building:   return Create<BuildingInteractionCommand>(protocol);
                case SceneObjectType.Chests:     return Create<ChestInteractionCommand>(protocol);
                case SceneObjectType.Rooms:      return Create<RoomBuyInteractor>(protocol);
                case SceneObjectType.OrderBoard: return Create<OrderBoardInteractionCommand>(protocol);
                case SceneObjectType.AntHill:    return Create<AntHillInteractCommand>(protocol);
                case SceneObjectType.Safe:       return Create<SafeInteractionCommand>(protocol);
                case SceneObjectType.Hospital:   return Create<HospitalInteractionCommand>(protocol);
                case SceneObjectType.Bowl:       return Create<BowlInteractionCommand>(protocol);
                default:                         return Task.CompletedTask;
            }
        }

        private Task Create<T>(InteractionProtocol protocol, params object[] args) where T : InteractionBaseCommand
        {
            return  _instantiator.Instantiate<T>(args).Execute(protocol);
        }
    }
}