using System;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingStatModel : IStorageItem
    {
        public string ModelID;
        public StatModel[] Stats;
        
        string IStorageItem.Id => ModelID;
    }
}