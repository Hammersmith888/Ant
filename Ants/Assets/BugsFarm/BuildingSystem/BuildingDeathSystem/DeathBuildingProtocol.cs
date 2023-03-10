using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    public struct DeathBuildingProtocol : IProtocol
    {
        public string Guid;
        public string DeathReason; //???
    }
}