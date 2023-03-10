using BugsFarm.Services.CommandService;

namespace BugsFarm.LeafHeapSystem
{
    public readonly struct CreateLeafHeapProtocol : IProtocol
    {
        public readonly string ModelID;
        public readonly string Guid;

        public CreateLeafHeapProtocol(string id, bool isModel)
        {
            if (isModel)
            {
                Guid = System.Guid.NewGuid().ToString();
                ModelID = id;
            }
            else
            {
                Guid = id;
                ModelID = null;
            }
        }
    }
}