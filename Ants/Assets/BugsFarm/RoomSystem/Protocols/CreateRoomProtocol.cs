using BugsFarm.Services.CommandService;

namespace BugsFarm.RoomSystem
{
    public readonly struct CreateRoomProtocol : IProtocol
    {
        public readonly string ModelID;
        public readonly string Guid;

        public CreateRoomProtocol(string id, bool isModel)
        {
            if (isModel)
            {
                ModelID = id;
                Guid = System.Guid.NewGuid().ToString();
            }
            else
            {
                ModelID = null;
                Guid = id;
            }
        }
    }
}