using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    [Serializable]
    public struct ResurrectBuildingData : IStorageItem
    {
        public string Id => Guid;
        public string Guid;
        public string ModelID;
    }
}