using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.Services.StatsService
{
    [Serializable]
    public class StatDto : IStorageItem
    {
        string IStorageItem.Id => StatId;
        public string StatId;
        public string StatType;

        public StatDto(StatModel initModel)
        {
            StatId = initModel.StatID;
            StatType = initModel.StatType;
        }
    }
}