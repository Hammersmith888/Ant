
namespace BugsFarm.InventorySystem
{
    public interface IInventorySlot
    {
        string ItemID { get; }
        int Count { get; }
        int Capacity { get; }
    }
}