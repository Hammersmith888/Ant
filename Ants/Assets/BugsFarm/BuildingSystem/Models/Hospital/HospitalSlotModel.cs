using System;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct HospitalSlotModel : IStorageItem
    {
        string IStorageItem.Id => ModelId;
        public string ModelId;
        public CurrencyModel Price;
    }
}