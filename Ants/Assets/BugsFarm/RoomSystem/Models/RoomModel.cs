using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.RoomSystem
{
    [Serializable]
    public struct RoomModel : IStorageItem
    {
        public string ModelID;
        public string TypeName;
        public CurrencyModel[] Price;
        
        string IStorageItem.Id => ModelID;
    }
}