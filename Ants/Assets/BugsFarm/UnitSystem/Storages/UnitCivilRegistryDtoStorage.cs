using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    public class UnitCivilRegistryDtoStorage : Storage<UnitCivilRegistryDto>
    {
        public UnitCivilRegistryDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}