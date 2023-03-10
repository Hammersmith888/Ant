using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitBirthModel : IStorageItem
    {
        public string ModelID;
        public string[] TrackingUnitsModelID;
        public string[] BirthUnitsModelID;
        public string LarvaModelID;
        
        string IStorageItem.Id => ModelID;
    }
}