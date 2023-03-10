using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class OrderDtoStorage : Storage<OrderDto>
    {
        public OrderDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}