using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    [Serializable]
    public struct QuestElementModel : IStorageItem
    {
        public string Id => ModelID;

        public string ModelID;
        public string LocalizationKey;
        public string QuestIcon;
        public int Level;
        public int GoldReward;
        public string QuestType;
        public int MinGoalValue;
        public int MaxGoalValue;
        public string ReferenceID;
        public float QuestDurationInMinutes;
    }
}