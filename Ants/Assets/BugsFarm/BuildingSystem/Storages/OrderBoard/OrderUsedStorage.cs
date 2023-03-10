using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class OrderUsedStorage : Storage<OrderUsed>
    {
        public OrderUsedStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}