using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct DeleteMoverProtocol : IProtocol
    {
        public readonly string Guid;
        public DeleteMoverProtocol(string guid)
        {
            Guid = guid;
        }
    }
}