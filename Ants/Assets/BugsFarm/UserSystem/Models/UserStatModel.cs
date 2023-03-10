using System;
using BugsFarm.Services.StatsService;

namespace BugsFarm.UserSystem
{
    [Serializable]
    public struct UserStatModel
    {
        public StatModel[] Stats;
    }
}