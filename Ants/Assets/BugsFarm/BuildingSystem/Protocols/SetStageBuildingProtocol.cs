using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct StageActionProtocol
    {
        public readonly int MaxIndex;
        public readonly int CurIndex;

        public StageActionProtocol(int maxIndex, int curIndex)
        {
            MaxIndex = maxIndex;
            CurIndex = curIndex;
        }
    }
    public readonly struct SetStageBuildingProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly float CurrValue;
        public readonly float MaxValue;
        public readonly Action<StageActionProtocol> StageIndexAction;

        public SetStageBuildingProtocol(string guid, float currValue, float maxValue, Action<StageActionProtocol> setStageindex = null)
        {
            Guid = guid;
            CurrValue = currValue;
            MaxValue = maxValue;
            StageIndexAction = setStageindex;
        }
    }
}