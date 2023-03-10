using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct SpeedUpBuildingProtocol : IProtocol
    {
        public readonly string Guid;
        public SpeedUpBuildingProtocol(string guid)
        {
            Guid = guid;
        }
    }
}