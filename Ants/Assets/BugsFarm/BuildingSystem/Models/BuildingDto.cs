using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class BuildingDto : IStorageItem
    {
        public string Guid;
        public string ModelID;
        public string PlaceNum;
        string IStorageItem.Id => Guid;
    }
}