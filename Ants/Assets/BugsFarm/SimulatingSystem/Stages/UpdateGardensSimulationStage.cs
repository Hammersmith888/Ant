using System.Collections.Generic;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class UpdateGardensSimulationStage
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;

        private const string _growTimeStatKey = "stat_growTime";

        public UpdateGardensSimulationStage(StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void UpdateGardens(float simulationTimeInMinutes, Dictionary<string, List<SimulatingEntityDto>> simulationData)
        {
            if (!simulationData.ContainsKey(SimulatingEntityType.Garden))
                return;
            
            foreach (var garden in simulationData[SimulatingEntityType.Garden])
            {
                StatsCollection statsCollection = _statsCollectionStorage.Get(garden.Guid);
                StatVital growTimeStat = statsCollection.Get<StatVital>(_growTimeStatKey);
                if (growTimeStat.CurrentValue - simulationTimeInMinutes <= 0)
                {
                    growTimeStat.CurrentValue = 0.01f;
                }
                else
                {
                    growTimeStat.CurrentValue -= simulationTimeInMinutes;
                }
                    
            }
        }
    }
}