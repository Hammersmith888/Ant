using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingRestockModel : IStorageItem
    {
        string IStorageItem.Id => ModelId;
        public string ModelId;
        public string ItemId;
        public CurrencyModel Price;
        public int MinPrecent;
        public int MaxPrecent;
    }
}