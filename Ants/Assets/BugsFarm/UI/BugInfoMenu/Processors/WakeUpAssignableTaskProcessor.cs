using BugsFarm.UnitSystem;

namespace BugsFarm.UI
{
    public class WakeUpAssignableTaskProcessor : IUnitAssignableTaskProcessor
    {
        private readonly UnitSleepSystem _unitSleepSystem;

        public WakeUpAssignableTaskProcessor(UnitSleepSystem unitSleepSystem)
        {
            _unitSleepSystem = unitSleepSystem;
        }

        public bool CanExecute(string guid)
        {
            return _unitSleepSystem.IsSleep(guid);
        }

        public void Execute(string guid)
        {
            _unitSleepSystem.Awake(guid);
        }
    }
}