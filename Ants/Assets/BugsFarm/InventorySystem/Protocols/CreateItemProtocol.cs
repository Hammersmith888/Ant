using System.Collections.Generic;
using BugsFarm.Services.CommandService;

namespace BugsFarm.InventorySystem
{
    public readonly struct CreateItemProtocol : IProtocol
    {
        public readonly string ItemID;
        public readonly int Count;
        public readonly List<IItem> OutItemList;

        public CreateItemProtocol(string itemID, int count, List<IItem> outItemList)
        {
            ItemID = itemID;
            Count = count;
            OutItemList = outItemList;
        }
    }
}