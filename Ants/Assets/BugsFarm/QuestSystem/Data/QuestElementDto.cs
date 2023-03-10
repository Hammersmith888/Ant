using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    [Serializable]
    public class QuestElementDto : IStorageItem
    {
        public string Id => Guid;

        public string Guid;
        public string ModelID;
        public bool IsStashed;
        public bool IsCompleted;
        public int CurrentValue;
        public int Level;
        public float TimeLeftForDiscarding;
        public int GoalValue;
        public string ReferenceID;
    }
}