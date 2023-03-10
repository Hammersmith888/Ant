using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitShopItemModel : IStorageItem
    {
        public string ModelID;
        public int BuyLevel;
        public CurrencyModel Price;
        
        string IStorageItem.Id => ModelID;
    }
}