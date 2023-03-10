using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class Wheat : BaseTrashFood
    {
        protected override AnimKey TakeAnim => AnimKey.TakeFoodLow;
    }
}