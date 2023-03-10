using System.Threading.Tasks;
using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public class OpenRoomCommand : ICommand<OpenRoomProtocol>
    {
        private readonly IRoomsSystem _roomsSystem;

        public OpenRoomCommand(IRoomsSystem roomsSystem)
        {
            _roomsSystem = roomsSystem;
        }
        
        public Task Execute(OpenRoomProtocol protocol)
        {
            _roomsSystem.OpenRoom(protocol.Guid);
            return Task.CompletedTask;
        }
    }
}