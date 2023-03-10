using BugsFarm.Services.CommandService;

namespace BugsFarm.LeafHeapSystem
{
    public readonly struct CreateLeafHeapSceneObjectProtocol : IProtocol
    {
        public readonly string Guid;

        public CreateLeafHeapSceneObjectProtocol(string guid)
        {
            Guid = guid;
        }
    }
}