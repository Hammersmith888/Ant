using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct AntHillTaskModel : IStorageItem
    {
        public string Id => TaskID;

        public string TaskID;
        public string[] ReferenceModelID;
        public string ReferenceGroup;
        public string Localization;
        public int Level;
        public string TaskType;
        public int CompletionGoal;
        public int RewardPoints;
        public string TaskIcon;
        public string ConditionName;
        public int ConditionValue;
        public string ProgressWay;
    }
}