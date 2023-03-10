using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct CreateUnitSceneObjectProtocol : IProtocol
    {
        public readonly string UnitId;
        public readonly string ModelId;

        public CreateUnitSceneObjectProtocol(string unitId, string modelId)
        {
            UnitId = unitId;
            ModelId = modelId;
        }
    }
}