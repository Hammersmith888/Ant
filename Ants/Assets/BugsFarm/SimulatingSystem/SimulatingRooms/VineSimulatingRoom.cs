using BugsFarm.InventorySystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class VineSimulatingRoom : ISimulatingRoom
    {
        public string Guid => _guid;
        public string ModelID => _modelID;

        public string Group => SimulatingRoomGroups.Vine;
        public int Capacity => _slot.Capacity;

        private string _guid;
        private readonly InventoryDto _inventoryDto;
        private readonly string _modelID;
        private readonly ItemSlot _slot;
        private const string _itemID = "5";
        
        public VineSimulatingRoom(string guid, string modelID, InventoryDto inventoryDto)
        {
            _guid = guid;
            _modelID = modelID;
            _inventoryDto = inventoryDto;
            _slot = _inventoryDto.Slots.Find(x => x.ItemID == _itemID);

        }

        public bool IsOpened()
        {
            return _slot.Count >= _slot.Capacity;
        }

        public void SetOpened()
        {
            _slot.Count = _slot.Capacity;
        }

        public int UpProgress(float percent)
        {
            int itemsToAdd = Mathf.RoundToInt(Mathf.Lerp(0, _slot.Capacity, percent / 100.0f));
            _slot.Count += itemsToAdd;
            return itemsToAdd;
        }
    }
}