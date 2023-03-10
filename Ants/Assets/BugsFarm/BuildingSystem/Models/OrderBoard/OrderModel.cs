using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct OrderModel : IStorageItem
    {
        string IStorageItem.Id => OrderID;
        public bool IsSpecial;
        public string OrderID;
        public float LifeTime;
        public int Level;
        public OrderItemModel[] Items;
    }
}