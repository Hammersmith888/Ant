using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class DrinkTaskAssigner : ITaskAssigner
    {
        private readonly UnitDrinkSystem _unitEatSystem;
        private readonly SimulatingTeleporter _simulatingTeleporter;

        public DrinkTaskAssigner(UnitDrinkSystem unitEatSystem, SimulatingTeleporter simulatingTeleporter)
        {
            _simulatingTeleporter = simulatingTeleporter;
            _unitEatSystem = unitEatSystem;
        }

        public bool CanAssign(string guid)
        {
            return !_unitEatSystem.IsHungry(guid);
        }

        public void Assign(string guid)
        {
            _simulatingTeleporter.TeleportToAny(guid,"39");
            _unitEatSystem.Start(guid);
        }
    }
}