using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class EatTaskAssigner : ITaskAssigner
    {
        private readonly UnitEatSystem _unitEatSystem;

        public EatTaskAssigner(UnitEatSystem unitEatSystem)
        {
            _unitEatSystem = unitEatSystem;
        }

        public bool CanAssign(string guid)
        {
            return !_unitEatSystem.IsHungry(guid);
        }

        public void Assign(string guid)
        {
            _unitEatSystem.Start(guid);
        }
    }
}