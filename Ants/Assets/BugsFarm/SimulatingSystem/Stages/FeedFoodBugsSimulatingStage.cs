using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class FeedFoodBugsSimulatingStage
    {
        private Dictionary<string, List<SimulatingEntityDto>> _simulationData;
        private readonly SimulatingFoodOrderModelStorage _foodOrderModelStorage;
        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private List<string> _buildingsToDelete;

        private float _minutesInCycle;

        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _maxResourceStatKey = "stat_maxResource";

        private const string _consumeFoodStatKey = "stat_consumeFood";
        private const string _noNeedFoodTimeStatKey = "stat_noNeedTimeFood";
        private const string _timeWithoutFoodStatKey = "stat_timeWithoutFood";
        
        public FeedFoodBugsSimulatingStage(StatsCollectionStorage statsCollectionStorage, 
            SimulatingFoodOrderModelStorage foodOrderModelStorage,
            BuildingDtoStorage buildingDtoStorage,
            ISimulatingEntityStorage simulatingEntityStorage,
            InventoryDtoStorage inventoryDtoStorage)
        {
            _inventoryDtoStorage = inventoryDtoStorage;
            _simulatingEntityStorage = simulatingEntityStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _foodOrderModelStorage = foodOrderModelStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public void FeedBugs(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulationData,
            Dictionary<string, List<DeathEntityProtocol>> deadUnits, List<string> buildingsToDelete)
        {
            _minutesInCycle = minutesInCycle;
            _buildingsToDelete = buildingsToDelete;
            _simulationData = simulationData;

            if (simulationData.ContainsKey(SimulatingEntityType.FoodStorage))
            {
                simulationData[SimulatingEntityType.FoodStorage] = simulationData[SimulatingEntityType.FoodStorage].OrderBy(foodStorage =>
                {
                    return _foodOrderModelStorage.Get(_buildingDtoStorage.Get(foodStorage.Guid).ModelID).Priority;
                }).Reverse().ToList();
            }
            
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
                DeathReason = DeathReason.Food
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
            StatsCollection statsCollection = _statsCollectionStorage.Get(simulatingEntityDto.Guid);
            
            if (!statsCollection.HasEntity(_noNeedFoodTimeStatKey))
                return true;
            if (!statsCollection.HasEntity(_consumeFoodStatKey))
                return true;
            if (!statsCollection.HasEntity(_timeWithoutFoodStatKey))
                return true;
            
            StatVital noNeedStat = statsCollection.Get<StatVital>(_noNeedFoodTimeStatKey);
            StatVital consumeStat = statsCollection.Get<StatVital>(_consumeFoodStatKey);

            SimulatingEater eater = new SimulatingEater()
            {
                NeedFoodLeft = consumeStat.BaseValue * _minutesInCycle / noNeedStat.Value
            };

            FeedFromSourceGroup(eater, SimulatingEntityType.Garden, false);

            if (eater.NeedFoodLeft == 0)
                return true;

            FeedFromSourceGroup(eater, SimulatingEntityType.FoodStorage, true);

            if (eater.NeedFoodLeft == 0)
                return true;

            return false;
        }
        

        private void FeedFromSourceGroup(SimulatingEater eater, string group, bool canBeRemoved)
        {
            if (!_simulationData.ContainsKey(group))
                return;
            
            foreach (var foodStorage in _simulationData[group].ToArray())
            {
                FeedFromSource(eater, foodStorage, canBeRemoved);

                if (eater.NeedFoodLeft == 0)
                    return;
            }
        }

        private void FeedFromSource(SimulatingEater eater, SimulatingEntityDto source, bool canBeRemoved)
        {
            InventoryDto sourceInventory = _inventoryDtoStorage.Get(source.Guid);
            ItemSlot slot = sourceInventory.Slots[0];
            float foodValue = slot.Count;
            float foodNeed = eater.NeedFoodLeft;
            eater.NeedFoodLeft = Mathf.Max(eater.NeedFoodLeft - foodValue, 0);
            slot.Count = Mathf.RoundToInt(Mathf.Max(foodValue - foodNeed , 0));
            if(slot.Count == 0 && canBeRemoved)
                RemoveFoodSource(source.Guid);
        }
        private void RemoveFoodSource(string guid)
        {
            if(_buildingsToDelete.Contains(guid))
                return;
            
            _buildingsToDelete.Add(guid);
            _simulatingEntityStorage.Remove(guid);
        }
        
        private class SimulatingEater
        {
            public float NeedFoodLeft;
        }
    }
}