using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.ChestSystem
{
    public class ChestDtoStorage : Storage<ChestDto>
    {
        public ChestDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}