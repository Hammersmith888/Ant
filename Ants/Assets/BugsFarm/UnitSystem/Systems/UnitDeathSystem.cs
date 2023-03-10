using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.InventorySystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using UniRx;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitDeathSystem
    {
        public event Action<string> OnUnitStartDie;

        /// <summary>
        /// arg1 = идентификатор юнита, arg2 = список статов для отслеживания
        /// </summary>
        private readonly Dictionary<string, DeathController> _storage;

        /// <summary>
        /// arg1 = Идентификатор юнита, arg2 = причина смерти
        /// </summary>
        private readonly Dictionary<string, string> _deadUnits;
        
        private readonly IInstantiator _instantiator;
        private readonly UnitCivilRegistrySystem _deathRegisterSystem;
        private readonly UnitTaskProcessorStorage _taskProcessorsStorage;
        private readonly StatsCollectionDtoStorage _statsCollectionDtoStorage;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IActivitySystem _activitySystem;
        private readonly PathHelper _pathHelper;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly List<string> _alredyDie;
        private readonly TaskMock _deathTaskMock;
        private readonly UnitMoverDtoStorage _moverDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        private const string _levelStat = "stat_level";
        
        public UnitDeathSystem(IInstantiator instantiator,
                               UnitCivilRegistrySystem deathRegisterSystem,
                               UnitTaskProcessorStorage taskProcessorsStorage,
                               UnitMoverStorage unitMoverStorage,
                               UnitDtoStorage unitDtoStorage,
                               UnitModelStorage unitModelStorage,
                               ISimulationSystem simulationSystem,
                               IActivitySystem activitySystem,
                               PathHelper pathHelper,
                               StatsCollectionDtoStorage statsCollectionDtoStorage,
                               UnitMoverDtoStorage moverDtoStorage,
                               StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _moverDtoStorage = moverDtoStorage;
            _instantiator = instantiator;
            _unitModelStorage = unitModelStorage;
            _deathRegisterSystem = deathRegisterSystem;
            _taskProcessorsStorage = taskProcessorsStorage;
            _simulationSystem = simulationSystem;
            _unitDtoStorage = unitDtoStorage;
            _activitySystem = activitySystem;
            _statsCollectionDtoStorage = statsCollectionDtoStorage;
            _pathHelper = pathHelper;
            _unitMoverStorage = unitMoverStorage;
            _storage = new Dictionary<string, DeathController>();
            _deadUnits = new Dictionary<string, string>();
            _alredyDie = new List<string>();
            _deathTaskMock = instantiator.Instantiate<TaskMock>(new object[] {nameof(DeathUnitBootstrapTask), false});
            MessageBroker.Default.Receive<DeathUnitProtocol>().Subscribe(OnDeadly);
        }

        public void Registration(string guid, params string[] statKeys)
        {
            if (HasUnit(guid))
            {
                throw new InvalidOperationException($"Registration Unit with [Guid : {guid}], alredy exist.");
            }
            
            _storage.Add(guid, _instantiator.Instantiate<DeathController>(new object[] {guid, statKeys}));
        }

        public void UnRegistration(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            UncheckStats(guid);

            if (IsDeadly(guid) && _taskProcessorsStorage.HasEntity(guid))
            {
                var taskProcessor = _taskProcessorsStorage.Get(guid);
                taskProcessor.Interrupt();
            }

            _storage.Remove(guid);
            _deadUnits.Remove(guid);
            _alredyDie.Remove(guid);
        }

        public bool HasUnit(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public bool IsDeadly(string guid)
        {
            return _deadUnits.ContainsKey(guid);
        }

        public bool IsRunned(string guid)
        {
            return _alredyDie.Contains(guid);
        }

        public void Start(string unitId)
        {
            if (!HasUnit(unitId)) return;

            if (!IsDeadly(unitId)) return;

            if (IsRunned(unitId)) return;

            if (!_taskProcessorsStorage.HasEntity(unitId)) return;
            
            var mover = _unitMoverStorage.Get(unitId);
            var unitDto = _unitDtoStorage.Get(unitId);
            var pathHelperQuery = PathHelperQuery.Empty()
                .UseGraphMask(GetType().Name)
                .UseLimitationsCheck(unitDto.ModelID)
                .UseTraversableCheck(mover.TraversableTags);
            var taskPoint = _pathHelper.GetRandomNodes(pathHelperQuery).First();
            var taskProcessor = _taskProcessorsStorage.Get(unitId);
            
            _deathTaskMock.SetTaskPoints(taskPoint);
            if (!taskProcessor.CanInterrupt(_deathTaskMock)) return;
            
            var args = new object[] { taskPoint.Position, new Action(() => StartFadeAction(unitId))};
            var deadTask = _instantiator.Instantiate<DeathUnitBootstrapTask>(args);
            
            UncheckStats(unitId);
            deadTask.OnComplete += _ => OnUnitDeathEnd(unitId);
            deadTask.OnInterrupt += _ => { _alredyDie.Remove(unitId); };

            _alredyDie.Add(unitId);
            taskProcessor.SetTask(deadTask);
            OnUnitStartDie?.Invoke(unitId);
        }

        private void UncheckStats(string guid)
        {
            if (!HasUnit(guid))
                return;

            var deathController = _storage[guid];
            deathController.Dispose();
        }

        private void StartFadeAction(string guid)
        {
            if (_deathRegisterSystem.HasUnit(guid))
            {
                throw new InvalidOperationException($"{this} : Unit was already dead.");
            }

            var dto = _unitDtoStorage.Get(guid);
            var mover = _moverDtoStorage.Get(guid);
            var deathReason = _deadUnits[guid];
            var statsDto = _statsCollectionDtoStorage.Get(guid);
            var unitLevel = (int)_statsCollectionStorage.Get(guid).GetValue(_levelStat);
            var deathRegistry = new UnitCivilRegistryDto
            {
                UnitDto = dto,
                UnitStatsDto = statsDto,
                UnitLevel = unitLevel,
                DeathReason = deathReason,
                DeathTime = _simulationSystem.GameAge,
                MoverDto = mover,
                PostLoad = false
            };
            _deathRegisterSystem.Registration(deathRegistry);
            MessageBroker.Default.Publish(new PostDeathUnitProtocol(){ UnitId = guid});
        }

        private void OnUnitDeathEnd(string guid)
        {
            if (!HasUnit(guid))
                return;

            _deadUnits.Remove(guid);
            _alredyDie.Remove(guid);
            if (_activitySystem.HasEntity(guid) && _activitySystem.IsActive(guid))
            {
                _activitySystem.Activate(guid,false);
            }

            var protocol = new DeleteUnitProtocol(guid);
            _instantiator.Instantiate<DeleteUnitCommand>().Execute(protocol);
        }

        public void OnDeadly(DeathUnitProtocol protocol)
        {
            if (!protocol.DeathReason.AnyOff(DeathReason.Food, DeathReason.Water)) return;
            if (!HasUnit(protocol.UnitId)) return;
            if (IsDeadly(protocol.UnitId)) return;

            _deadUnits.Add(protocol.UnitId, protocol.DeathReason);
            Start(protocol.UnitId);
        }
    }
}