using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.LeafHeapSystem
{
    [Serializable]
    public class LeafHeapDto : IStorageItem
    {
        public string ModelID;
        public string Guid;
        
        string IStorageItem.Id => Guid;

        public LeafHeapDto(string modelID, string guid)
        {
            ModelID = modelID;
            Guid = guid;
        }
    }
}