using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.LeafHeapSystem
{
    public class LeafHeapDtoStorage : Storage<LeafHeapDto>
    {
        public LeafHeapDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}