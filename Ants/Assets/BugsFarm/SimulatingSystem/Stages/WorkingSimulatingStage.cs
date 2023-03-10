using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem
{
    public class WorkingSimulatingStage
    {
        private Dictionary<string, List<SimulatingEntityDto>> _simulatingData;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitDtoStorage _unitDtoStorage;

        private List<string> _unfilledBuildings;
        private float _minutesInCycle;

        private const string _workerModelID = "8";

        private const string _capacityStatKey = "stat_capacity";
        private const string _efficiencyStatKey = "stat_efficiency";

        public WorkingSimulatingStage(StatsCollectionStorage statsCollectionStorage,
            UnitDtoStorage unitDtoStorage)
        {
            _unitDtoStorage = unitDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _unfilledBuildings = new List<string>();
        }

        public void Work(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulatingData)
        {
            if (!simulatingData.ContainsKey(SimulatingEntityType.WorkBuilding))
                return;
            if (!simulatingData.ContainsKey(SimulatingEntityType.Unit))
                return;
            var workingUnits = simulatingData[SimulatingEntityType.Unit]
                .Where(x => _unitDtoStorage.Get(x.Guid).ModelID == _workerModelID);
            var workingUnitsList = workingUnits.ToList();
            if (workingUnitsList.Count == 0)
                return;

            _minutesInCycle = minutesInCycle;
            _simulatingData = simulatingData;
            
            _unfilledBuildings.Clear();
            GetUnfilledBuildings();
            for (int i = 0; i < workingUnitsList.Count; i++)
            { 
                if(i >= _unfilledBuildings.Count)
                    break;
                float percent = GetPercentOfDayForFilling(_unfilledBuildings[i]);
                if (percent < 1.0f)
                {
                    FillBuilding(_unfilledBuildings[i]);
                }
                else
                {
                    AddPercentOfResource(_unfilledBuildings[i], percent - 1.0f);
                }
            }
        }

   
        private void GetUnfilledBuildings()
        {
            foreach (var buildingDto in _simulatingData[SimulatingEntityType.WorkBuilding])    
            {
                StatsCollection statsCollection = _statsCollectionStorage.Get(buildingDto.Guid);
                StatVital capacity = statsCollection.Get<StatVital>(_capacityStatKey);
                if(capacity.CurrentValue < capacity.Value)
                    _unfilledBuildings.Add(buildingDto.Guid);
            }
        }

        private void FillBuilding(string buildingGuid)
        {
            StatsCollection statsCollection = _statsCollectionStorage.Get(buildingGuid);
            StatVital capacity = statsCollection.Get<StatVital>(_capacityStatKey);
            capacity.CurrentValue = capacity.Value;
        }

        private void AddPercentOfResource(string buildingGuid, float percentToAdd)
        {
            StatsCollection statsCollection = _statsCollectionStorage.Get(buildingGuid);
            StatVital capacity = statsCollection.Get<StatVital>(_capacityStatKey);
            capacity.CurrentValue = Mathf.Min(capacity.Value, capacity.CurrentValue * (1.0f + percentToAdd));
        }
        
        private float GetPercentOfDayForFilling(string buildingGuid)
        {
            StatsCollection statsCollection = _statsCollectionStorage.Get(buildingGuid);
            StatVital capacity = statsCollection.Get<StatVital>(_capacityStatKey);
            StatModifiable efficiency = statsCollection.Get<StatModifiable>(_efficiencyStatKey);
            var efficiencyPerDay = efficiency.Value * _minutesInCycle;
            var percentOfWorkDay= (capacity.Value - capacity.CurrentValue) / efficiencyPerDay;
            return percentOfWorkDay;
        }
    }
}