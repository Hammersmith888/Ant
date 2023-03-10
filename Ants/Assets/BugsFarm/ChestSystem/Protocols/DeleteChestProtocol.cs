using BugsFarm.Services.CommandService;

namespace BugsFarm.ChestSystem
{
    public readonly struct DeleteChestProtocol : IProtocol
    {
        public readonly string Guid;

        public DeleteChestProtocol(string guid)
        {
            Guid = guid;
        }
    }
}