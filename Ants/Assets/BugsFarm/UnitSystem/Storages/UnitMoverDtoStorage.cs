using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    public class UnitMoverDtoStorage : Storage<UnitMoverDto>
    {
        public UnitMoverDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}