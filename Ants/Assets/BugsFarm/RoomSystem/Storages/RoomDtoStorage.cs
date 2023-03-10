using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.RoomSystem
{
    public class RoomDtoStorage : Storage<RoomDto>
    {
        public RoomDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}