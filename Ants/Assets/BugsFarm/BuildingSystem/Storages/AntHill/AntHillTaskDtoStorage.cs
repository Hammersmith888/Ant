using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class AntHillTaskDtoStorage : Storage<AntHillTaskDto>
    {
        public AntHillTaskDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
        
    }
}