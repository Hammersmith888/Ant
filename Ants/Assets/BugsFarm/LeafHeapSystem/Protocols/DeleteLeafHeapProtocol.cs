using BugsFarm.Services.CommandService;

namespace BugsFarm.LeafHeapSystem
{
    public readonly struct DeleteLeafHeapProtocol : IProtocol
    {
        public readonly string Guid;

        public DeleteLeafHeapProtocol(string guid)
        {
            Guid = guid;
        }
    }
}