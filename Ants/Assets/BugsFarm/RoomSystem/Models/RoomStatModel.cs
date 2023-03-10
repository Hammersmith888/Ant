using System;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.RoomSystem
{
    [Serializable]
    public struct RoomStatModel : IStorageItem
    {
        public string ModelID;
        public StatModel[] Stats;
        
        string IStorageItem.Id => ModelID;
    }
}