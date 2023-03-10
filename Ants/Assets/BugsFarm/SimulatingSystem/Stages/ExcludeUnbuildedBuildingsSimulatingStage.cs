using System.Collections.Generic;
using BugsFarm.Services.StatsService;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class ExcludeUnbuildedBuildingsSimulatingStage
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private List<string> _buildingsToBuild;

        private const string _buildingTimeStatKey = "stat_buildingTime";

        public ExcludeUnbuildedBuildingsSimulatingStage(StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void CleanFromUnbuildedBuildings(float minutesInCycle,
            Dictionary<string, List<SimulatingEntityDto>> collection, List<string> buildingsToBuild)
        {
            _buildingsToBuild = buildingsToBuild;
            if (collection.ContainsKey(SimulatingEntityType.WorkBuilding))
                CleanBuildings(minutesInCycle, collection[SimulatingEntityType.WorkBuilding]);
            if (collection.ContainsKey(SimulatingEntityType.Training))
                CleanBuildings(minutesInCycle, collection[SimulatingEntityType.Training]);
            if (collection.ContainsKey(SimulatingEntityType.GarbageCollector))
                CleanBuildings(minutesInCycle, collection[SimulatingEntityType.GarbageCollector]);
            if (collection.ContainsKey(SimulatingEntityType.Garden))
                CleanBuildings(minutesInCycle, collection[SimulatingEntityType.Garden]);
            if (collection.ContainsKey(SimulatingEntityType.OrderBoard))
                CleanBuildings(minutesInCycle, collection[SimulatingEntityType.OrderBoard]);
        }

        private void CleanBuildings(float minutesInCycle, List<SimulatingEntityDto> buildingDtos)
        {
            for (int i = 0; i < buildingDtos.Count; i++)
            {
                if(!_statsCollectionStorage.HasEntity(buildingDtos[i].Guid))
                    continue;
                
                var statCollection = _statsCollectionStorage.Get(buildingDtos[i].Guid);

               if (!statCollection.HasEntity(_buildingTimeStatKey))
                   continue;
               
               if(!statCollection.TryGet<StatVital>(_buildingTimeStatKey, out StatVital buildingTimeStat))
                   continue;
               
               var guid = buildingDtos[i].Guid;
               if (buildingTimeStat.CurrentValue > minutesInCycle)
               {
                   int lastIndex = buildingDtos.Count - 1;
                   (buildingDtos[i], buildingDtos[lastIndex]) = (buildingDtos[lastIndex], buildingDtos[i]);
                   buildingDtos.RemoveAt(lastIndex);
                   i -= 1;
               }
               else
               {
                   if(!_buildingsToBuild.Contains(guid))
                       _buildingsToBuild.Add(guid);
               }

               buildingTimeStat.CurrentValue -= Mathf.Max(0, minutesInCycle);

            }
        }
        
    }
}