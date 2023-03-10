using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public readonly struct CreateBuildingSceneObjectProtocol : IProtocol
    {
        public readonly string Guid;

        public CreateBuildingSceneObjectProtocol(string guid)
        {
            Guid = guid;
        }
    }
}