using System;
using UnityEngine;

namespace BugsFarm.InventorySystem
{
    [Serializable]
    public class ItemSlot : IInventorySlot
    {
        public string ItemID
        {
            get => _itemID;
            internal set => _itemID = value;
        }

        public int Count
        {
            get => _count;
            set => _count = value;
        }

        public int Capacity
        {
            get => _capacity;
            internal set => _capacity = value;
        }

        [SerializeField] private string _itemID;
        [SerializeField] private int _count;
        [SerializeField] private int _capacity;

        public ItemSlot(string itemID, int count = 0, int capacity = -1)
        {
            _itemID = itemID;
            _count = count;
            _capacity = capacity;
        }
    }
}