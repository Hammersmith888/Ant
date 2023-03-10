using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public struct OpenRoomProtocol : IProtocol
    {
        public string Guid;

        public OpenRoomProtocol(string guid)
        {
            Guid = guid;
        }
    }
}