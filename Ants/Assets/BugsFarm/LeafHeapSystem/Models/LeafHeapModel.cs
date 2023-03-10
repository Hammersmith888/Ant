using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.LeafHeapSystem
{
    [Serializable]
    public struct LeafHeapModel : IStorageItem
    {
        public string ModelID;
        
        string IStorageItem.Id => ModelID;
    }
}