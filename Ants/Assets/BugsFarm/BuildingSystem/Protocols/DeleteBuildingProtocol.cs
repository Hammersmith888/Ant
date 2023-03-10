using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct DeleteBuildingProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly bool Notify;
        public DeleteBuildingProtocol(string guid, bool notify = true)
        {
            Guid = guid;
            Notify = notify;
        }
    }
}