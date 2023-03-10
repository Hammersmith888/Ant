using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class PostLoadRoomsCommand : Command
    {
        private readonly IRoomsSystem _roomsSystem;
        private readonly IInstantiator _instantiator;
        private readonly RoomDtoStorage _roomDtoStorage;
        
        public PostLoadRoomsCommand(IRoomsSystem roomsSystem,
                                    IInstantiator instantiator,
                                    RoomDtoStorage roomDtoStorage)
        {
            _roomsSystem = roomsSystem;
            _instantiator = instantiator;
            _roomDtoStorage = roomDtoStorage;
        }
        public override void Do()
        {
            var buildRoomCommand = _instantiator.Instantiate<CreateRoomCommand>();
            foreach (var roomDto in _roomDtoStorage.Get())
            {
                var buildRoomProtocol = new CreateRoomProtocol(roomDto.Guid, false);
                buildRoomCommand.Execute(buildRoomProtocol);
            }

            _roomsSystem.Initialize();
            OnDone();
        }
    }
}