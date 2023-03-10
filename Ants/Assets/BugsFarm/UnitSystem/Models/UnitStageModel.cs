using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitStageModel : IStorageItem
    {
        public string ModelID;
        public string Path;
        public int Count;
        
        string IStorageItem.Id => ModelID;
    }
}