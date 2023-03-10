using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.InventorySystem
{
    [Serializable]
    public struct InventoryItemModel : IStorageItem
    {
        public string ItemID;
        string IStorageItem.Id => ItemID;
    }
}