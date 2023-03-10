using System.Collections;
using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using BugsFarm.BuildingSystem.DeathSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingProcess : ISimulationProcess
    {
        private readonly CheckSafeFullnessSimulatingStage _checkSafeFullnessSimulatingStage;
        private readonly ExcludeUnbuildedBuildingsSimulatingStage _excludeBuildingsStage;
        private readonly UnitsAssignTaskSimulationStage _unitsAssignTaskSimulationStage;
        private readonly UnitsRepositionSimulatingStage _unitsRepositionSimulatingStage;
        private readonly FeedWaterBugsSimulatingStage _feedWaterBugsSimulatingStage;
        private readonly UpdateGardensSimulationStage _updateGardensSimulationStage;
        private readonly UpdateOrdersSimulatingStage _updateOrdersSimulatingStage;
        private readonly FeedFoodBugsSimulatingStage _feedFoodBugsSimulatingStage;
        private readonly BirthLarvaSimulatingStage _birthLarvaSimulatingStage;
        private readonly DailyQuestSimulatingStage _dailyQuestSimulatingStage;
        private readonly SimulatingMudStockCreator _simulatingMudStockCreator;
        private readonly SimulatingAntLarvaCreator _simulatingAntLarvaCreator;
        private readonly GrowLarvaSimulatingStage _growLarvaSimulatingStage;
        private readonly OpenRoomsSimulatingStage _openRoomsSimulatingStage;
        private readonly BuildingStatModelStorage _buildingStatModelStorage;
        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly TrainingSimulatingStage _trainingSimulatingStage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly WorkingSimulatingStage _workingSimulatingStage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly SimulatingUnitsKiller _unitsKiller;
        private readonly IInstantiator _instantiator;

        private Dictionary<string, List<SimulatingEntityDto>> _simulationData;
        private Dictionary<string, List<DeathEntityProtocol>> _deadEntities;
        private List<SimulatingSpawnUnitDto> _unitsToSpawn;
        private List<string> _buildingsToBuild;
        private List<string> _larvaToDelete;
        private List<string> _buildingsToDelete;

        private const string _maxGarbageStatKey = "stat_maxGrabage";
        private const string _maxResourceStatKey = "stat_maxResource";
        private const string _garbageStockModelID = "45";
        
        public SimulatingProcess(ExcludeUnbuildedBuildingsSimulatingStage excludeBuildingsStage,
                                 ISimulatingEntityStorage simulatingEntityStorage,
                                 CheckSafeFullnessSimulatingStage checkSafeFullnessSimulatingStage,
                                 UpdateGardensSimulationStage updateGardensSimulationStage,
                                 FeedFoodBugsSimulatingStage feedFoodBugsSimulatingStage,
                                 FeedWaterBugsSimulatingStage feedWaterBugsSimulatingStage,
                                 OpenRoomsSimulatingStage openRoomsSimulatingStage,
                                 SimulatingMudStockCreator simulatingMudStockCreator,
                                 SimulatingAntLarvaCreator simulatingAntLarvaCreator,
                                 BirthLarvaSimulatingStage birthLarvaSimulatingStage,
                                 GrowLarvaSimulatingStage growLarvaSimulatingStage,
                                 TrainingSimulatingStage trainingSimulatingStage,
                                 UnitsAssignTaskSimulationStage unitsAssignTaskSimulationStage,
                                 DailyQuestSimulatingStage dailyQuestSimulatingStage,
                                 WorkingSimulatingStage workingSimulatingStage,
                                 StatsCollectionStorage statsCollectionStorage,
                                 BuildingDtoStorage buildingDtoStorage,
                                 SimulatingUnitsKiller unitsKiller,
                                 BuildingStatModelStorage buildingStatModelStorage,
                                 UpdateOrdersSimulatingStage updateOrdersSimulatingStage,
                                 UnitsRepositionSimulatingStage unitsRepositionSimulatingStage,
                                 IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _unitsKiller = unitsKiller;
            _dailyQuestSimulatingStage = dailyQuestSimulatingStage;
            _unitsRepositionSimulatingStage = unitsRepositionSimulatingStage;
            _updateOrdersSimulatingStage = updateOrdersSimulatingStage;
            _buildingStatModelStorage = buildingStatModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _workingSimulatingStage = workingSimulatingStage;
            _trainingSimulatingStage = trainingSimulatingStage;
            _birthLarvaSimulatingStage = birthLarvaSimulatingStage;
            _growLarvaSimulatingStage = growLarvaSimulatingStage;
            _simulatingMudStockCreator = simulatingMudStockCreator;
            _simulatingAntLarvaCreator = simulatingAntLarvaCreator;
            _feedWaterBugsSimulatingStage = feedWaterBugsSimulatingStage;
            _feedFoodBugsSimulatingStage = feedFoodBugsSimulatingStage;
            _updateGardensSimulationStage = updateGardensSimulationStage;
            _checkSafeFullnessSimulatingStage = checkSafeFullnessSimulatingStage;
            _unitsAssignTaskSimulationStage = unitsAssignTaskSimulationStage;
            _excludeBuildingsStage = excludeBuildingsStage;
            _simulatingEntityStorage = simulatingEntityStorage;
            _openRoomsSimulatingStage = openRoomsSimulatingStage;
            _deadEntities = new Dictionary<string, List<DeathEntityProtocol>>();
            _unitsToSpawn = new List<SimulatingSpawnUnitDto>();
            _larvaToDelete = new List<string>();
            _buildingsToDelete = new List<string>();
            _buildingsToBuild = new List<string>();
        }
        public void Simulate(float minutesInCycle, float dayModifier, float cycleNum)
        {
            _simulationData = _simulatingEntityStorage.CreateTemporaryDatabase();
            _excludeBuildingsStage.CleanFromUnbuildedBuildings(minutesInCycle, _simulationData, _buildingsToBuild);
            _checkSafeFullnessSimulatingStage.CheckSafeFullness(minutesInCycle, _simulationData);
            _updateGardensSimulationStage.UpdateGardens(minutesInCycle, _simulationData);
            _feedFoodBugsSimulatingStage.FeedBugs(minutesInCycle, _simulationData, _deadEntities, _buildingsToDelete);
            _feedWaterBugsSimulatingStage.FeedBugs(minutesInCycle, _simulationData, _deadEntities);
            _openRoomsSimulatingStage.OpenRooms(minutesInCycle, _simulationData, dayModifier);
            _birthLarvaSimulatingStage.BirthLarvas(minutesInCycle, _simulationData, cycleNum);
            _growLarvaSimulatingStage.GrowLarvas(minutesInCycle, _larvaToDelete, _unitsToSpawn, _simulationData);
            _trainingSimulatingStage.TrainUnits(minutesInCycle, _simulationData);
            _workingSimulatingStage.Work(minutesInCycle, _simulationData);
        }

        public void SimulateOneTime(double simulatingTime, double previousGameAge)
        {
            _updateOrdersSimulatingStage.UpdateOrders(simulatingTime, previousGameAge);
        }
        
        public void PostSimulate(double simulatingTime, double previousGameAge)
        {
            RemoveLarvas();
            RemoveBuildings();
            SpawnUnits();
            _simulatingMudStockCreator.CreateMudStockSceneObjects();
            _simulatingAntLarvaCreator.CreateLarvasSceneObjects();
            KillUnits();
            KillQueen();
            _unitsRepositionSimulatingStage.RelocateUnits(_simulationData);
            _dailyQuestSimulatingStage.ReduceDailyQuestTimers(simulatingTime, previousGameAge); 
            //_unitsAssignTaskSimulationStage.AssignTasksToUnits(simulatingTime, previousGameAge);
        }

        private void RemoveLarvas()
        {
            for (int i = 0; i < _larvaToDelete.Count; i++)
            {
                _simulatingEntityStorage.Remove(_larvaToDelete[i]);
                var protocol = new DeleteUnitProtocol(_larvaToDelete[i]);
                var command = _instantiator.Instantiate<DeleteUnitCommand>();
                command.Execute(protocol);
            }
        }

        private void RemoveBuildings()
        {
            for (int i = 0; i < _buildingsToDelete.Count; i++)
            {
                StatsCollection statsCollection = _statsCollectionStorage.Get(_buildingsToDelete[i]);
                var placeNum = _buildingDtoStorage.Get(_buildingsToDelete[i]).PlaceNum;

                DeleteBuildingProtocol protocol = new DeleteBuildingProtocol(_buildingsToDelete[i]);
                _instantiator.Instantiate<DeleteBuildingCommand>().Execute(protocol);

                if (statsCollection.HasEntity(_maxGarbageStatKey))
                {
                    var garbageValue = statsCollection.GetVitalValue(_maxGarbageStatKey);
                    var createBuildingProtocol = new CreateBuildingProtocol(_garbageStockModelID, placeNum, true);

                    CreateStatsCollectionProtocol createStatsCollectionProtocol =
                        new CreateStatsCollectionProtocol(createBuildingProtocol.Guid,
                            _buildingStatModelStorage.Get(_garbageStockModelID).Stats);

                    _instantiator.Instantiate<CreateStatsCollectionCommand<BuildingStatsCollection>>()
                        .Execute(createStatsCollectionProtocol);

                    var statCollection = _statsCollectionStorage.Get(createBuildingProtocol.Guid);
                    StatVital mudAmountStat = statCollection.Get<StatVital>(_maxResourceStatKey);
                    mudAmountStat.CurrentValue = garbageValue;

                    var slot = new ItemSlot("3", (int) mudAmountStat.CurrentValue, (int) mudAmountStat.Value);
                    var inventoryProtocol = new CreateInventoryProtocol(createBuildingProtocol.Guid, res => { }, slot);
                    _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

                    _instantiator.Instantiate<CreateBuildingCommand>().Execute(createBuildingProtocol);
                }
            }
        }

        private void SpawnUnits()
        {
            for (int i = 0; i < _unitsToSpawn.Count; i++)
            {
                var spawnProtocol = new UnitSpawnProtocol(_unitsToSpawn[i].Guid);
                _instantiator.Instantiate<UnitSpawnCommand<SpawnFromAlphaTask>>().Execute(spawnProtocol);
            }
        }

        private void KillUnits()
        {
            if (_deadEntities.ContainsKey(SimulatingEntityType.Unit))
            {
                _unitsKiller.KillBugs(_deadEntities[SimulatingEntityType.Unit]);
            }
        }

        private void KillQueen()
        {
            if (_deadEntities.ContainsKey(SimulatingEntityType.Queen))
            {
                var data = _deadEntities[SimulatingEntityType.Queen][0];
                MessageBroker.Default.Publish(new DeathBuildingProtocol() {Guid = data.Guid, DeathReason = data.DeathReason});
            }
        }

        public void Dispose()
        {
            _deadEntities = null;
            _buildingsToBuild = null;
            _buildingsToDelete = null;
            _unitsToSpawn = null;
            _larvaToDelete = null;
            _openRoomsSimulatingStage.Dispose();
            _simulatingMudStockCreator.Dispose();
            _trainingSimulatingStage.Dispose();
            _checkSafeFullnessSimulatingStage.Dispose();
        }
    }
}