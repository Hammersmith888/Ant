namespace BugsFarm.BuildingSystem
{
    public class AddFeedQueenTask : AddBuildingNeedTask
    {
        public override bool Interruptible => false;
    }
}