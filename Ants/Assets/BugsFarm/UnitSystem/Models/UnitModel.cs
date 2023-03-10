using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitModel : IStorageItem
    {
        public string ModelID;
        public string TypeName;
        public bool IsFemale;
        
        string IStorageItem.Id => ModelID;
    }
}