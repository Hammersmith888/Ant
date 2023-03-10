namespace BugsFarm.TaskSystem
{
    public class TaskInfo
    {
        public string TaskName { get; }
        public string TaskGuid { get; }
        public string OwnerId { get; }
        public string OwnerModelID { get; }
        public string TaskHash { get; }
        public TaskParams Requirements { get; }
        public TaskParams GivesReward { get; }
        
        public TaskInfo(string taskName, string ownerId, string ownerModelID, ITask task)
        {
            OwnerId = ownerId;
            TaskName = taskName;
            TaskGuid = task.Guid;
            OwnerModelID = ownerModelID;
            Requirements = task.GetRequirements();
            GivesReward = task.GetRewards();
            TaskHash = OwnerId + Requirements.GetCustomHashCode() + GivesReward.GetCustomHashCode();
        }
    }
}