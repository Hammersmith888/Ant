using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct PrisonSlotModel : IStorageItem
    {
        public string Id => ModelId;
        public string ModelId;
        public string AssetPath;
    }
}