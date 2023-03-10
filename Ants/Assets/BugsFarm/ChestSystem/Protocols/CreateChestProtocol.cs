using BugsFarm.Services.CommandService;

namespace BugsFarm.ChestSystem
{
    public readonly struct CreateChestProtocol : IProtocol
    {
        public readonly string ModelID;
        public readonly string Guid;

        public CreateChestProtocol(string id, bool isModel)
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