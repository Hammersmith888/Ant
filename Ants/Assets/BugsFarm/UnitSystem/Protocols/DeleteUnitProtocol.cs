using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct DeleteUnitProtocol : IProtocol
    {
        public readonly string Guid;

        public DeleteUnitProtocol(string guid)
        {
            Guid = guid;
        }
    }
}