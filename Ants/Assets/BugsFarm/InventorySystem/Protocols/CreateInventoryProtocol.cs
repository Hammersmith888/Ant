using System;
using BugsFarm.Services.CommandService;

namespace BugsFarm.InventorySystem
{
    public readonly struct CreateInventoryProtocol : IProtocol
    {
        public readonly string Guid;
        public readonly Action<IInventory> CallbackResult;
        public readonly ItemSlot[] Items;

        public CreateInventoryProtocol(string guid, params ItemSlot[] itemSlots)
        {
            Guid = guid;
            Items = itemSlots;
            CallbackResult = null;
        }

        public CreateInventoryProtocol(string guid, Action<IInventory> callbackResult, params ItemSlot[] itemSlots)
        {
            Guid = guid;
            CallbackResult = callbackResult;
            Items = itemSlots;
        }
    }
}