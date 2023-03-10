using System;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.ChestSystem
{
    [Serializable]
    public struct ChestStatModel : IStorageItem
    {
        public string ModelID;
        public StatModel[] Stats;
        
        string IStorageItem.Id => ModelID;
    }
}