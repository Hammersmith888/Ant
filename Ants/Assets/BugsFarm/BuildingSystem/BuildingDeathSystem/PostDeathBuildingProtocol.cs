using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    public struct PostDeathBuildingProtocol : IProtocol
    {
        public string Guid;
        public string ModelID;
    }
}