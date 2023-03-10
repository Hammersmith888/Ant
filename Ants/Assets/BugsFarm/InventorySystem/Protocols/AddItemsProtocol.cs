using BugsFarm.Services.CommandService;

namespace BugsFarm.InventorySystem
{
    public readonly struct AddItemsProtocol : IProtocol
    {
        public readonly string ItemID;
        public readonly int Count;
        public readonly string Guid;

        public AddItemsProtocol(string itemID, int count, string guid)
        {
            ItemID = itemID;
            Count = count;
            Guid = guid;
        }
    }
}