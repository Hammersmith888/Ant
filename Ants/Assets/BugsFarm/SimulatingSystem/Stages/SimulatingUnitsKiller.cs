using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingUnitsKiller
    {
        private readonly StatsCollectionDtoStorage _statsCollectionDtoStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly UnitCivilRegistrySystem _unitCivilRegistrySystem;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitMoverDtoStorage _moverDtoStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly IActivitySystem _activitySystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly PathHelper _pathHelper;

        private const string _levelStat = "stat_level";

        public SimulatingUnitsKiller(UnitSceneObjectStorage unitSceneObjectStorage,
            IActivitySystem activitySystem,
            UnitCivilRegistrySystem unitCivilRegistrySystem,
            UnitDtoStorage unitDtoStorage,
            StatsCollectionDtoStorage statsCollectionDtoStorage,
            UnitMoverDtoStorage unitMoverDtoStorage,
            StatsCollectionStorage statsCollectionStorage,
            ISimulationSystem simulationSystem,
            IInstantiator instantiator,
            PathHelper pathHelper,
            UnitMoverStorage unitMoverStorage, 
            UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _unitMoverStorage = unitMoverStorage;
            _pathHelper = pathHelper;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _statsCollectionDtoStorage = statsCollectionDtoStorage;
            _unitCivilRegistrySystem = unitCivilRegistrySystem;
            _unitSceneObjectStorage = unitSceneObjectStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _moverDtoStorage = unitMoverDtoStorage;
            _activitySystem = activitySystem;
            _unitDtoStorage = unitDtoStorage;
        }

        public void KillBugs(List<DeathEntityProtocol> unitsToKill)
        {
            foreach (var unitToKill in unitsToKill)
            {
                KillUnit(unitToKill);
            }
        }

        private void KillUnit(DeathEntityProtocol unitToKill)
        {
            var mover = _unitMoverStorage.Get(unitToKill.Guid);
            var layer = mover.Layer;
            var unitDto = _unitDtoStorage.Get(unitToKill.Guid);
            var pathHelperQuery = PathHelperQuery.Empty()
                .UseGraphMask("UnitDeathSystem")
                .UseLimitationsCheck(unitDto.ModelID)
                .UseTraversableCheck(mover.TraversableTags);
            var taskPoint = _pathHelper.GetRandomNodes(pathHelperQuery).First();

            var moveTask = _instantiator.Instantiate<MoveToScenePointTask>(new object[] {taskPoint.Position});
            moveTask.Execute(unitToKill.Guid);
            moveTask.ForceComplete();
            //_unitTaskProcessorStorage.Get(unitToKill.Guid).Interrupt();

            var view = _unitSceneObjectStorage.Get(unitToKill.Guid);
            _moverDtoStorage.Get(unitToKill.Guid).Layer = layer;
            
            view.SetLayer(layer);

            view.SetInteraction(false);
            view.SetActive(false);
            RemoveFromActivitySystem(unitToKill);
            RegisterToCivilSystem(unitToKill);
            var protocol = new DeleteUnitProtocol(unitToKill.Guid);
            _instantiator.Instantiate<DeleteUnitCommand>().Execute(protocol);
            MessageBroker.Default.Publish(new PostDeathUnitProtocol(){ UnitId = unitToKill.Guid});
        }

        private void RegisterToCivilSystem(DeathEntityProtocol unit)
        {
            var dto = _unitDtoStorage.Get(unit.Guid);
            var mover = _moverDtoStorage.Get(unit.Guid);
            var deathReason = unit.DeathReason;
            var statsDto = _statsCollectionDtoStorage.Get(unit.Guid);
            var unitLevel = (int)_statsCollectionStorage.Get(unit.Guid).GetValue(_levelStat);
            var deathRegistry = new UnitCivilRegistryDto
            {
                UnitDto = dto,
                UnitStatsDto = statsDto,
                UnitLevel = unitLevel,
                DeathReason = deathReason,
                DeathTime = _simulationSystem.GameAge,
                MoverDto = mover,
                PostLoad = true
            };
            _unitCivilRegistrySystem.Registration(deathRegistry);
        }

        private void RemoveFromActivitySystem(DeathEntityProtocol unitToKill)
        {
            if (_activitySystem.HasEntity(unitToKill.Guid) && _activitySystem.IsActive(unitToKill.Guid))
            {
                _activitySystem.Activate(unitToKill.Guid, false);
            }
        }

    }
}