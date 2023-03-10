using BugsFarm.Services.CommandService;

namespace BugsFarm.InventorySystem
{
    public readonly struct DeleteInventoryProtocol : IProtocol
    {
        public readonly string Guid;
        public DeleteInventoryProtocol(string guid)
        {
            Guid = guid;
        }
    }
}