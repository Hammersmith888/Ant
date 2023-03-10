using BugsFarm.AnimationsSystem;

namespace BugsFarm.BuildingSystem
{
    public class Sugar : BaseTrashFood
    {
        protected override AnimKey TakeAnim => AnimKey.TakeFoodLow;
    }
}