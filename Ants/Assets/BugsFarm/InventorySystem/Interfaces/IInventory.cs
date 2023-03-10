using System;
using System.Collections.Generic;
using BugsFarm.Services.StorageService;

namespace BugsFarm.InventorySystem
{
    public interface IInventory : IStorageItem, IDisposable
    {
        event Action<string> OnCreatedItemSlot;
        event Action<string> UpdateInventoryAction;
        void AddItem(IItem item);
        void AddItems(IEnumerable<IItem> items);
        IEnumerable<IItem> GetItems(string itemId, ref int count);
        IEnumerable<IItem> GetItems(string itemId);
        IInventorySlot GetItemSlot(string itemId);
        void SetSlotCapacity(string itemId, int capacity);
        void SetDefaultCapacity(int capacity);
        int GetDefaultCapacity();
        void Remove(string itemId, int count);
        void Remove(string itemId);
        bool HasItem(string itemId, int count = 0);
        bool HasItemSlot(string itemId);
    }
}