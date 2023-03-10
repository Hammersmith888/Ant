using System;
using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitCivilRegistryDto : IStorageItem
    {
        public UnitDto UnitDto;
        public StatsCollectionDto UnitStatsDto;
        public int UnitLevel;
        public double DeathTime;
        public string DeathReason;
        public UnitMoverDto MoverDto;
        public bool PostLoad;
        public string Id => UnitDto.Guid;
    }
}