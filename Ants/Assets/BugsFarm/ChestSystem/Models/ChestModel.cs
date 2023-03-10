using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.ChestSystem
{
    [Serializable]
    public struct ChestModel : IStorageItem
    {
        public string ModelID;
        public int TypeID;
        public CurrencyModel[] Items;
        
        string IStorageItem.Id => ModelID;
    }
}