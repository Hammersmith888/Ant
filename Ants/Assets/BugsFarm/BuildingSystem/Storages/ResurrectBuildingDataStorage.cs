using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class ResurrectBuildingDataStorage : Storage<ResurrectBuildingData>
    {
        public ResurrectBuildingDataStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
        
    }
}