using System;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitStatModel : IStorageItem
    {
        public string ModelID;
        public StatModel[] Stats;
        
        string IStorageItem.Id => ModelID;
    }
}