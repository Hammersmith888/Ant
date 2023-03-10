using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.InventorySystem
{
    [Serializable]
    public class InventoryDto : IStorageItem
    {
        public string Id => _guid;
        public List<ItemSlot> Slots => _slots;
        
        [SerializeField] private string _guid;
        [SerializeField] private List<ItemSlot> _slots;
        
        public InventoryDto(string guid, IEnumerable<ItemSlot> slots)
        {
            _guid = guid;
            _slots = slots?.ToList() ?? new List<ItemSlot>();
        }
    }
}