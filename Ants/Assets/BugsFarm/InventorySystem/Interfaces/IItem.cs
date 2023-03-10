namespace BugsFarm.InventorySystem
{
    public interface IItem
    {
        int Count { get; set; }
        string ItemID { get; }
    }
}