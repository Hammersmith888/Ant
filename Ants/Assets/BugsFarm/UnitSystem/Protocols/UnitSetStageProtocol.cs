using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public readonly struct UnitSetStageProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly int StageIndex;

        public UnitSetStageProtocol(string guid, int stageIndex)
        {
            Guid = guid;
            StageIndex = stageIndex;
        }
    }
}