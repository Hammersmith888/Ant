using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingSpeedupModel : IStorageItem
    {
        string IStorageItem.Id => ModelId;
        public string ModelId;
        public CurrencyModel Price;
    }
}