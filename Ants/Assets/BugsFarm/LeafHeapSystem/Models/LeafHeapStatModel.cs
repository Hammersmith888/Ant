using System;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.LeafHeapSystem
{
    [Serializable]
    public struct LeafHeapStatModel : IStorageItem
    {
        public string ModelID;
        public StatModel[] Stats;
        
        string IStorageItem.Id => ModelID;
    }
}