using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingStageModel : IStorageItem
    {
        public string ModelID;
        public string Path;
        public int Count;
        
        string IStorageItem.Id => ModelID;
    }
}