using BugsFarm.Services.CommandService;

namespace BugsFarm.BuildingSystem
{
    public struct HospitalRemoveSlotProtocol : IProtocol
    {
        public string SlotId;
    }
}