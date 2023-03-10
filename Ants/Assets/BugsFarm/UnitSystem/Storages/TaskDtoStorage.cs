using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    public class TaskDtoStorage : Storage<TaskDto>
    {
        public TaskDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}