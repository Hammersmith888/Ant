using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct UpgradeBuildingProtocol : IProtocol
    {
        public readonly string BuildingId;

        public UpgradeBuildingProtocol(string buildingId)
        {
            BuildingId = buildingId;
        }
    }
}