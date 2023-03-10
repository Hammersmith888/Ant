namespace BugsFarm.TaskSystem
{
    public class FindRequest
    {
        public string WithHash;
        public string[] WithoutGuids = new string[0];
        public string[] WithoutModelIDs = new string[0];
        public string[] WithModelIDs = new string[0];
        public string[] WithGivesReward = new string[0];
        public bool WithoutRequirements = false;
    }
}