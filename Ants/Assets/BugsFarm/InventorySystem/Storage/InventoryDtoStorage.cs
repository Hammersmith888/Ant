using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.InventorySystem
{
    public class InventoryDtoStorage : Storage<InventoryDto>
    {
        public InventoryDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}