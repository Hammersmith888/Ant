using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public class TaskDto : IStorageItem
    {
        public string UnitGuid;
        public string TaskHash;
        
        string IStorageItem.Id => UnitGuid;
    }
}