using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.FarmCameraSystem
{
    public class CinematicsResolvedStatesStorage : Storage<CinematicsState>
    {
        public CinematicsResolvedStatesStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}