using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingModel : IStorageItem
    {
        public string ModelID; 
        public int TypeID;     
        public string TypeName;
        public bool CanOverlap;
        
        string IStorageItem.Id => ModelID;
    }
}