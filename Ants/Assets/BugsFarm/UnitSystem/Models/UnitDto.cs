using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public class UnitDto : IStorageItem
    {
        public string Guid;
        public string ModelID;
        public int NameID;

        public UnitDto(string guid, string modelID, int nameID)
        {
            Guid = guid;
            ModelID = modelID;
            NameID = nameID;
        }
        
        string IStorageItem.Id => Guid;
    }
}