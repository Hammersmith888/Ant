using BugsFarm.InventorySystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class DigSimulatingRoom : ISimulatingRoom
    {
        public string Guid => _guid;
        public string ModelID => _modelID;
        
        public string Group => SimulatingRoomGroups.Dig;
        public int Capacity => _inventoryDto.Slots.Find(x => x.ItemID == _itemID).Capacity;

        private string _guid;
        private readonly InventoryDto _inventoryDto;
        private readonly string _modelID;
        private const string _itemID = "2";

        public DigSimulatingRoom(string guid, string modelID, InventoryDto inventoryDto)
        {
            _guid = guid;
            _modelID = modelID;
            _inventoryDto = inventoryDto;
        }

        public bool IsOpened()
        {
            return !_inventoryDto.Slots.Exists(x => x.ItemID == _itemID);
        }

        public void SetOpened()
        {
            for (int i = 0; i < _inventoryDto.Slots.Count; i++)
            {
                if(_inventoryDto.Slots[i].ItemID != _itemID)
                    continue;

                var lastIndex = _inventoryDto.Slots.Count - 1;
                (_inventoryDto.Slots[i], _inventoryDto.Slots[lastIndex]) = (_inventoryDto.Slots[lastIndex], _inventoryDto.Slots[i]);
                _inventoryDto.Slots.RemoveAt(lastIndex);
                i -= 1;
            }
        }

        public int UpProgress(float percent)
        {
            var slot = _inventoryDto.Slots.Find(x => x.ItemID == _itemID);
            int itemsToRemove = Mathf.RoundToInt(Mathf.Lerp(0, slot.Capacity, percent / 100.0f));
            slot.Count -= itemsToRemove;
            if(slot.Count <= 0)
                _inventoryDto.Slots.Remove(slot);
            return itemsToRemove;
        }
    }
}