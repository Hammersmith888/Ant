using BugsFarm.Services.CommandService;

namespace BugsFarm.ChestSystem
{
    public readonly struct CreateChestSceneObjectPrtotocol : IProtocol
    {
        public readonly string Guid;

        public CreateChestSceneObjectPrtotocol(string guid)
        {
            Guid = guid;
        }
    }
}