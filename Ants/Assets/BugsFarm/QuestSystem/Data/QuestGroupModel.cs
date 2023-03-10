using System;
using System.Collections.Generic;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    [Serializable]
    public struct QuestGroupModel : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public double Duration;
        public List<VirtualChestModel> VirtualChestModels;
    }
}