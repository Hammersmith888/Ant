using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class AntHillTaskDto : IStorageItem
    {
        public string Id => TaskID;
        
        public string TaskID;
        public string[] ReferenceModelID;
        public string ReferenceGroup;
        public string TaskType;
        public int CompletionGoal;
        public string ConditionName;
        public int ConditionValue;
        public string ProgressWay;
        
        public int CurrentValue;

        
        public bool IsCompleted() => CurrentValue >= CompletionGoal;
    }
}