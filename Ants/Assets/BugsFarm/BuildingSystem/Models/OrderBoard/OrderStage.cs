using System;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public enum OrderStage
    {
        Process = 1,
        Reward = 2,
        Rewarding = 3,
        WaitNextOrder = 4,
        Finalized = 5
    }

    [Serializable]
    public enum OrderItemStage
    {
        Idle = 0,
        Process = 1,
        Compelte = 2
    }
}