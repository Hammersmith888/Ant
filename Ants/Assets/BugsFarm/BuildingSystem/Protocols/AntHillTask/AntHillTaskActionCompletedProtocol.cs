using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public struct AntHillTaskActionCompletedProtocol : IProtocol
    {
        public string EntityType;
        public AntHillTaskType TaskType;
        public string ModelID;
        public string Guid;
    }
}