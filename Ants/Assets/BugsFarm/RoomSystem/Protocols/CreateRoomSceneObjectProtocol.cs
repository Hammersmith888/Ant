using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public readonly struct CreateRoomSceneObjectProtocol : IProtocol
    {
        public readonly string Guid;

        public CreateRoomSceneObjectProtocol(string guid)
        {
            Guid = guid;
        }
    }
}