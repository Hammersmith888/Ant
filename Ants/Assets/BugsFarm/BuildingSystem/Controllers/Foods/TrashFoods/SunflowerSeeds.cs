using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class SunflowerSeeds : BaseTrashFood
    {
        protected override AnimKey TakeAnim => AnimKey.TakeFoodLow;
    }
}