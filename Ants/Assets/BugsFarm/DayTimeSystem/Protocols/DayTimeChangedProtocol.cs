using BugsFarm.Services.CommandService;

namespace BugsFarm.DayTimeSystem
{
    public struct DayTimeChangedProtocol : IProtocol
    {
        public DayTime TimeOfDay;
    }
}