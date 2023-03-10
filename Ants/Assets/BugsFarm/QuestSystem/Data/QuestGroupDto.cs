using System;
using System.Collections.Generic;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Quest
{
    [Serializable]
    public class QuestGroupDto : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public List<VirtualChestDto> VirtualChests;

        public QuestGroupDto()
        {
            VirtualChests = new List<VirtualChestDto>();
        }
    }

    [Serializable]
    public class VirtualChestDto
    {
        public string ModelID;
        public int Reward;
        public string CurrencyID;
        public float Treshold;
        public bool IsOpened;
    }

    [Serializable]
    public struct VirtualChestModel
    {
        public string ModelID;
        public int Reward;
        public string CurrencyID;
        public float Treshold;
    }
}