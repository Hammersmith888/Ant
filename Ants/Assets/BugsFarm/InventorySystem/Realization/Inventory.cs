using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;
using UnityEngine;
using Zenject;

namespace BugsFarm.InventorySystem
{
    public class Inventory : IInventory
    {
        public event Action<string> OnCreatedItemSlot;
        public event Action<string> UpdateInventoryAction;

        private readonly IInstantiator _instantiator;
        private readonly InventoryDto _inventoryDto;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly Dictionary<string, ItemSlot> _inventory;
        private int _defaultCapacity = -1;

        private bool _batchAdd;
        private bool _finalized;

        string IStorageItem.Id => _inventoryDto.Id;

        public Inventory(IInstantiator instantiator, InventoryDto inventoryDto)
        {
            _instantiator = instantiator;
            _inventoryDto = inventoryDto;
            _inventory = _inventoryDto.Slots.ToDictionary(x => x.ItemID);
        }

        public void AddItem(IItem item)
        {
            if(_finalized) return;
            
            if (item.IsNullOrDefault())
            {
                throw new NullReferenceException($"{this} : AddItem :: Item is null");
            }

            if (!_inventory.ContainsKey(item.ItemID))
            {
                var itemSlot = new ItemSlot(item.ItemID, 0, _defaultCapacity);
                _inventory.Add(item.ItemID, itemSlot);
                _inventoryDto.Slots.Add(itemSlot);
                OnCreatedItemSlot?.Invoke(item.ItemID);
            }

            var slot = _inventory[item.ItemID];
            var summ = slot.Count + item.Count;
            if (slot.Capacity > 0) // слот имеет ограничение вместимости, зависит от параметра носителя
            {
                if (summ <= slot.Capacity)
                {
                    slot.Count = summ;
                    item.Count = 0;
                }
                else
                {
                    slot.Count = slot.Capacity;
                    item.Count = summ - slot.Capacity;
                }
            }
            else // безграничный слот
            {
                slot.Count = summ;
                item.Count = 0;
            }

            if (!_batchAdd)
            {
                UpdateInventoryAction?.Invoke(item.ItemID);
            }
        }

        public void AddItems(IEnumerable<IItem> items)
        {
            if(_finalized) return;
            if (items == null)
            {
                throw new ArgumentException($"{this} :  AddItems :: items is null");
            }

            _batchAdd = true;
            var itemId = "-1";
            foreach (var item in items)
            {
                AddItem(item);
                itemId = item.ItemID;
            }

            _batchAdd = false;

            if (itemId == "-1") return;
            UpdateInventoryAction?.Invoke(itemId);
        }

        public IEnumerable<IItem> GetItems(string itemId, ref int count)
        {
            if (_finalized) return default;
            if (!HasItem(itemId))
            {
                return default;
            }

            var hasCount = _inventory[itemId].Count;
            count = hasCount < count ? hasCount : count;

            var itemList = new List<IItem>();
            var protocol = new CreateItemProtocol(itemId, count, itemList);
            var command = _instantiator.Instantiate<CreateItemCommand>();
            command.Execute(protocol);
            return itemList;
        }

        public IEnumerable<IItem> GetItems(string itemId)
        {
            if (!HasItem(itemId) || _finalized) return default;

            var slot = _inventory[itemId];
            var count = slot.Count;
            return GetItems(itemId, ref count);
        }

        public IInventorySlot GetItemSlot(string itemId)
        {
            if (_finalized) return default;
            return HasItem(itemId, -1) ? _inventory[itemId] : default;
        }

        public void SetSlotCapacity(string itemId, int capacity)
        {
            if(_finalized) return;
            if (HasItem(itemId, -1))
            {
                _inventory[itemId].Capacity = capacity;
            }
        }

        public void SetDefaultCapacity(int capacity)
        {
            if(_finalized) return;
            _defaultCapacity = capacity;
            var slots = _inventory.Values.ToArray();
            foreach (var slot in slots)
            {
                slot.Capacity = _defaultCapacity;
            }
        }

        public int GetDefaultCapacity()
        {
            return _defaultCapacity;
        }

        public void Remove(string itemId, int count)
        {
            if (!HasItem(itemId) || _finalized || count == 0) return;

            var slot = _inventory[itemId];
            slot.Count = Mathf.Max(0, slot.Count - count);
            UpdateInventoryAction?.Invoke(itemId);
        }

        public void Remove(string itemId)
        {
            if (!HasItem(itemId) || _finalized) return;
            Remove(itemId, _inventory[itemId].Count);
        }

        public bool HasItem(string itemId, int count = 0)
        {
            if (_finalized) return false;
            return _inventory.ContainsKey(itemId) && _inventory[itemId].Count > count;
        }

        public bool HasItemSlot(string itemId)
        {
            return _inventory.ContainsKey(itemId);
        }

        public void Dispose()
        {
            if (_finalized) return;
            
            _finalized = true;
            OnCreatedItemSlot = null;
            UpdateInventoryAction = null;
            _inventory.Clear();
        }
    }
}