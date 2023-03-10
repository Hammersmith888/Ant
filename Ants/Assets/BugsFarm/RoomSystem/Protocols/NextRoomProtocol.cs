using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public struct NextRoomProtocol : IProtocol
    {
        public string Guid;
        public NextRoomProtocol(string guid)
        {
            Guid = guid;
        }
    }
}