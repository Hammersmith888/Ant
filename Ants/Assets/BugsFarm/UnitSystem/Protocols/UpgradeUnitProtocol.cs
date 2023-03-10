using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct UpgradeUnitProtocol : IProtocol
    {
        public readonly string UnitId;

        public UpgradeUnitProtocol(string unitId)
        {
            UnitId = unitId;
        }
    }
}