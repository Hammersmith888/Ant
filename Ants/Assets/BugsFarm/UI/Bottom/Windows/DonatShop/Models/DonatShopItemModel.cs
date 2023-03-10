using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UI
{
    [Serializable]
    public struct DonatShopItemModel : IStorageItem
    {
        string IStorageItem.Id => ModelId;

        public string ModelId;
        public string TypeId;
        public int Price;
        public int Count;
        public string IconName;
    }
}