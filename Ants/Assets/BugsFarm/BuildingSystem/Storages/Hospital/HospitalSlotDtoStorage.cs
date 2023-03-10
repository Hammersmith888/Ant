using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    public class HospitalSlotDtoStorage : Storage<HospitalSlotDto>
    {
        public HospitalSlotDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}