using BugsFarm.TaskSystem;

namespace BugsFarm.BuildingSystem
{
    public class AddDealerFarmTask : AddResourceBootstrapTask
    {
        protected override void OnInitialized()
        {
            var itemID = ResourceController.ItemID;
            var itemCount = ResourceController.NeedItemCount.ToString();
            Requirements = new TaskParams(new TaskParamModel(TaskParamID.ItemID, itemID, itemCount));
        }
    }
}