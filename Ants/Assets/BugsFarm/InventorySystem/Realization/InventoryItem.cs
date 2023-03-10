namespace BugsFarm.InventorySystem
{
    public class InventoryItem : IItem
    {
        public string ItemID { get; }
        public int Count { get; set;}
        public InventoryItem(string itemID, int count)
        {
            ItemID = itemID;
            Count = count;
        }
    }
}