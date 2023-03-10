using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    public class UnitDtoStorage : Storage<UnitDto>
    {
        public UnitDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}