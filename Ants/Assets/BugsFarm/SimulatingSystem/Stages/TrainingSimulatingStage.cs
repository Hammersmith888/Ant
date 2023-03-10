using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem
{
    public class TrainingSimulatingStage
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly SimulatingTrainingModelStorage _simulatingTrainingModelStorage;
        
        private readonly Dictionary<string, List<string>> _trainingBuildings;
        private readonly Dictionary<string, float> _unitTimers;
        private const string _unitExpStatKey = "stat_xp";
        private const string _experienceStatKey = "stat_experience";
        private const float _defaultTimer = 1440.0f;
        
        public TrainingSimulatingStage(StatsCollectionStorage statsCollectionStorage,
            BuildingDtoStorage buildingDtoStorage,
            UnitDtoStorage unitDtoStorage,
            SimulatingTrainingModelStorage simulatingTrainingModelStorage)
        {
            _simulatingTrainingModelStorage = simulatingTrainingModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _trainingBuildings = new Dictionary<string, List<string>>();
            _unitTimers = new Dictionary<string, float>();
        }

        public void TrainUnits(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulatingData)
        {
            if (!simulatingData.ContainsKey(SimulatingEntityType.Training))
                return;
            if (!simulatingData.ContainsKey(SimulatingEntityType.Unit))
                return;

            ConfigureTrainingBuildings(simulatingData[SimulatingEntityType.Training]);
            AddUnitTimers(simulatingData[SimulatingEntityType.Unit]);
            
            foreach (var unitEntityDto in simulatingData[SimulatingEntityType.Unit])
            {
                _unitTimers[unitEntityDto.Guid] -= minutesInCycle;
                if (_unitTimers[unitEntityDto.Guid] <= 0)
                {
                    TrainUnit(unitEntityDto.Guid);
                    _unitTimers[unitEntityDto.Guid] = _defaultTimer;
                }
            }
        }
        private void TrainUnit(string guid)
        {
            var unitModelID = _unitDtoStorage.Get(guid).ModelID;

            if (!_simulatingTrainingModelStorage.HasEntity(unitModelID))
                return;

            var randomTargetBuildingsModelID = _simulatingTrainingModelStorage.Get(unitModelID).BuildingsModelID;
            var buildingModelID = randomTargetBuildingsModelID[UnityEngine.Random.Range(0, randomTargetBuildingsModelID.Length)];

            if (!_trainingBuildings.ContainsKey(buildingModelID))
                return;

            var targetBuildingGuid =
                _trainingBuildings[buildingModelID][UnityEngine.Random.Range(0, _trainingBuildings[buildingModelID].Count)];
            
            
            var unitStatCollection = _statsCollectionStorage.Get(guid);
            var buildingStatsCollection = _statsCollectionStorage.Get(targetBuildingGuid);
            var unitExpStat = unitStatCollection.Get<StatVital>(_unitExpStatKey);

            unitExpStat.CurrentValue += buildingStatsCollection.GetValue(_experienceStatKey);
        }
        
        private void AddUnitTimers(List<SimulatingEntityDto> unitDtos)
        {
            foreach (var unitDto in unitDtos)
            {
                if (!_unitTimers.ContainsKey(unitDto.Guid))
                {
                    _unitTimers.Add(unitDto.Guid, _defaultTimer);
                }
            }
            
        }
        
        private void ConfigureTrainingBuildings(List<SimulatingEntityDto> simulatingEntityDtos)
        {
            _trainingBuildings.Clear();

            foreach (var entityDto in simulatingEntityDtos)
            {
                var modelID = _buildingDtoStorage.Get(entityDto.Guid).ModelID;
                if(!_trainingBuildings.ContainsKey(modelID))
                    _trainingBuildings.Add(modelID, new List<string>());
                _trainingBuildings[modelID].Add(entityDto.Guid);
            }
        }

        public void Dispose()
        {
            _trainingBuildings.Clear();
            _unitTimers.Clear();
        }
    }
}