using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class BuildingDtoStorage : Storage<BuildingDto>
    {
        public BuildingDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}