using System.Collections.Generic;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class FeedWaterBugsSimulatingStage
    {
        private Dictionary<string, List<SimulatingEntityDto>> _simulationData;
        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly InventoryDtoStorage _inventoryDtoStorage;

        private float _minutesInCycle;

        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _maxResourceStatKey = "stat_maxResource";

        private const string _consumeWaterStatKey = "stat_consumeWater";
        private const string _noNeedWaterTimeStatKey = "stat_noNeedTimeWater";
        private const string _timeWithoutWaterStatKey = "stat_timeWithoutWater";
        
        public FeedWaterBugsSimulatingStage(StatsCollectionStorage statsCollectionStorage, 
            ISimulatingEntityStorage simulatingEntityStorage,
            InventoryDtoStorage inventoryDtoStorage)
        {
            _inventoryDtoStorage = inventoryDtoStorage;
            _simulatingEntityStorage = simulatingEntityStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void FeedBugs(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulationData, Dictionary<string, List<DeathEntityProtocol>> deadUnits)
        {
            _minutesInCycle = minutesInCycle;
            _simulationData = simulationData;

            if (!simulationData.ContainsKey(SimulatingEntityType.Queen))
                return;
            
            if (!FeedBug(simulationData[SimulatingEntityType.Queen][0]))
            {
                var data = simulationData[SimulatingEntityType.Queen][0];
                KillBugWithinSimulationProcess(data);
                AddToDead(data, deadUnits);
            }
            
            if (!simulationData.ContainsKey(SimulatingEntityType.Unit))
                return;
            
            foreach (var unit in simulationData[SimulatingEntityType.Unit].ToArray())
            {
                if (!FeedBug(unit))
                {
                    KillBugWithinSimulationProcess(unit);
                    AddToDead(unit, deadUnits);
                }
            }
        }
        private void AddToDead(SimulatingEntityDto data, Dictionary<string, List<DeathEntityProtocol>> deadUnits)
        {
            if(!deadUnits.ContainsKey(data.EntityType))
                deadUnits.Add(data.EntityType, new List<DeathEntityProtocol>());
            deadUnits[data.EntityType].Add(new DeathEntityProtocol()
            {
                Guid = data.Guid,
                DeathReason = DeathReason.Water
            });
        }

        private void KillBugWithinSimulationProcess(SimulatingEntityDto simulatingEntityDto)
        {
            _simulationData[simulatingEntityDto.EntityType].Remove(simulatingEntityDto);
            if (_simulationData[simulatingEntityDto.EntityType].Count == 0)
                _simulationData.Remove(simulatingEntityDto.EntityType);
            _simulatingEntityStorage.Remove(simulatingEntityDto.Guid);
        }
        
        private bool FeedBug(SimulatingEntityDto simulatingEntityDto)
        {
            if (!_simulationData.ContainsKey(SimulatingEntityType.Bowl))
                return false;
            
            StatsCollection statsCollection = _statsCollectionStorage.Get(simulatingEntityDto.Guid);

            if (!statsCollection.HasEntity(_noNeedWaterTimeStatKey))
                return true;
            if (!statsCollection.HasEntity(_consumeWaterStatKey))
                return true;
            if (!statsCollection.HasEntity(_timeWithoutWaterStatKey))
                return true;
            
            StatVital noNeedStat = statsCollection.Get<StatVital>(_noNeedWaterTimeStatKey);
            StatVital consumeStat = statsCollection.Get<StatVital>(_consumeWaterStatKey);
            StatVital timeWithoutStat = statsCollection.Get<StatVital>(_timeWithoutWaterStatKey);
            
            
            if (noNeedStat == null || consumeStat == null || timeWithoutStat == null)
            {
                return true;
            }

            SimulatingDrinker drinker = new SimulatingDrinker()
            {
                NeedWaterLeft = consumeStat.Value * _minutesInCycle / noNeedStat.Value
            };

            FeedFromBowl(drinker);

            if (drinker.NeedWaterLeft == 0)
                return true;

            return false;
        }
        
        private void FeedFromBowl(SimulatingDrinker drinker)
        {
           // StatsCollection statsCollection = _statsCollectionStorage.Get(_simulationData[SimulatingEntityType.Bowl][0].Guid);
           // StatVital resourceValue = statsCollection.Get<StatVital>(_maxResourceStatKey);
            InventoryDto sourceInventory = _inventoryDtoStorage.Get(_simulationData[SimulatingEntityType.Bowl][0].Guid);
            ItemSlot slot = sourceInventory.Slots[0];
            float foodValue = slot.Count;
            float foodNeed = drinker.NeedWaterLeft;
            drinker.NeedWaterLeft = Mathf.Max(drinker.NeedWaterLeft - foodValue, 0);
            slot.Count = Mathf.RoundToInt(Mathf.Max(foodValue - foodNeed , 0));
        }
        
        private class SimulatingDrinker
        {
            public float NeedWaterLeft;
        }
    }
}