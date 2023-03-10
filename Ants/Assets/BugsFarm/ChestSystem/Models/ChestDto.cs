using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.ChestSystem
{
    [Serializable]
    public class ChestDto : IStorageItem
    {
        public string ModelID;
        public string Guid;
        
        string IStorageItem.Id => Guid;

        public ChestDto(string modelID, string guid)
        {
            ModelID = modelID;
            Guid = guid;
        }
    }
}