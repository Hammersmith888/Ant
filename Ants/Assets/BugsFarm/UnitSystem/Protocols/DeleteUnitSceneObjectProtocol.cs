using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct DeleteUnitSceneObjectProtocol : IProtocol
    {
        public readonly string UnitId;

        public DeleteUnitSceneObjectProtocol(string unitId)
        {
            UnitId = unitId;
        }
    }
}