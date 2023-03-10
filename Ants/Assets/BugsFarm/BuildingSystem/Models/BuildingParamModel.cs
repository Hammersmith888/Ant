using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingParamModel : IStorageItem
    {
        string IStorageItem.Id => ModelID;
        public string ModelID;
        public string[] Params;
    }
}