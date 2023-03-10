using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public struct OpenableRoomProtocol : IProtocol
    {
        public string Guid;
        public OpenableRoomProtocol(string guid)
        {
            Guid = guid;
        }
    }
}