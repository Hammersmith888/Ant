using System.Collections.Generic;
using System.Linq;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using BugsFarm.Utility;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class Hospital : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IInstantiator _instantiator;
        private readonly IActivitySystem _activitySystem;
        private readonly TaskStorage _taskStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingBuildSystem _buildingBuildSystem;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly HospitalSlotDtoStorage _hospitalSlotDtoStorage;
        private readonly HospitalSlotModelStorage _hospitalSlotModelStorage;
        private readonly UnitMoverStorage _unitMoverStorage;

        private const string _ladyBugNeedTextKey = "BuildingInner_48_LadyBug";
        private const string _ladyBugInsideTextKey = "BuildingInner_48_LadyBugInside";
        private const string _ladyBugModelId = "3";
        
        private const string _betwenHelpTimeStatKey = "stat_betwenHelpTime";
        private const string _repairTimeStatKey = "stat_repairTime";
        private const string _lifeTimeStatKey = "stat_lifeTime";

        private readonly Dictionary<string, ITask> _reserves;
        private readonly Dictionary<string, ITask> _repairs;
        private IEnumerable<IPosSide> _tasksPoints;
        private StatsCollection _statsCollection;
        private CompositeDisposable _events;
        private StateInfo _stateInfo;
        private BuildingDto _selfDto;

        private ITask _bugWorkTask;
        private ITask _bugWorkPauseTimer;
        private StatVital _betwenHelpTime;
        private bool _finalized;


        public Hospital(string guid,
                        IInstantiator instantiator,
                        IActivitySystem activitySystem,
                        BuildingDtoStorage buildingDtoStorage,
                        BuildingSceneObjectStorage buildingSceneObjectStorage,
                        StatsCollectionStorage statsCollectionStorage,
                        BuildingBuildSystem buildingBuildSystem,
                        UnitDtoStorage unitDtoStorage,
                        HospitalSlotDtoStorage hospitalSlotDtoStorage,
                        HospitalSlotModelStorage hospitalSlotModelStorage,
                        UnitMoverStorage unitMoverStorage,
                        TaskStorage taskStorage)
        {
            _instantiator = instantiator;
            _activitySystem = activitySystem;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingBuildSystem = buildingBuildSystem;
            _unitDtoStorage = unitDtoStorage;
            _hospitalSlotDtoStorage = hospitalSlotDtoStorage;
            _hospitalSlotModelStorage = hospitalSlotModelStorage;
            _unitMoverStorage = unitMoverStorage;
            _taskStorage = taskStorage;
            Id = guid;
            _events = new CompositeDisposable();
            _repairs = new Dictionary<string, ITask>();
            _reserves = new Dictionary<string, ITask>();
        }

        public void Initialize()
        {
            if(_finalized) return;
            var view = _buildingSceneObjectStorage.Get(Id);
            _statsCollection = _statsCollectionStorage.Get(Id);
            _tasksPoints = view.GetComponent<TasksPoints>().Points;
            _selfDto = _buildingDtoStorage.Get(Id);
            _betwenHelpTime = _statsCollection.Get<StatVital>(_betwenHelpTimeStatKey);
            _stateInfo = _instantiator.Instantiate<StateInfo>(new object[] {Id});
            
            _buildingBuildSystem.OnStarted += OnBuildStarted;
            _buildingBuildSystem.OnCompleted += OnBuildCompleted;
            _unitDtoStorage.OnStorageChanged += OnUnitStorageChanged;
            _stateInfo.OnUpdate += OnStateInfoUpdate;
            
            _buildingBuildSystem.Registration(Id);
            MessageBroker.Default.Receive<DeathUnitProtocol>()
                .Subscribe(OnDeathUnitEventHandler)
                .AddTo(_events);
            MessageBroker.Default.Receive<HospitalSlotRepairProtocol>()
                .Subscribe(OnRepairSlotEventHandler)
                .AddTo(_events);
            MessageBroker.Default.Receive<HospitalRemoveSlotProtocol>()
                .Subscribe(OnRemoveSlotEventHandler)
                .AddTo(_events);


            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
            }
            else
            {
                InitBugWorkPauseTimer();
                ReserveProduction();
                RepairProduction();
            }
        }

        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            
            _buildingBuildSystem.OnStarted -= OnBuildStarted;
            _buildingBuildSystem.OnCompleted -= OnBuildCompleted;
            _buildingBuildSystem.UnRegistration(Id);
            _unitDtoStorage.OnStorageChanged -= OnUnitStorageChanged;
            _stateInfo.OnUpdate -= OnStateInfoUpdate;
            
            BugWorkProduction();
            RepairProduction();
            ReserveProduction();
            
            foreach (var slotDto in _hospitalSlotDtoStorage.Get().ToArray())
            {
                _hospitalSlotDtoStorage.Remove(slotDto.Id);
                DeleteUnit(slotDto.Id);
            }
            
            _stateInfo?.Dispose();
            _stateInfo = null;
            _betwenHelpTime = null;
            _events?.Dispose();
            _events = null;
        }

        private bool CanProduction()
        {
            return !_buildingBuildSystem.IsBuilding(Id) &&
                   _unitDtoStorage.Get().Any(unitDto => unitDto.ModelID == _ladyBugModelId && 
                                                        !_hospitalSlotDtoStorage.HasEntity(unitDto.Guid));
        }
        
        private void BugWorkProduction()
        {
            if (_finalized || !CanProduction())
            {
                _bugWorkTask?.ForceComplete();
                return;
            }
            if (_bugWorkTask == null && _bugWorkPauseTimer == null)
            {
                var args = new object[] {Id, _tasksPoints};
                _bugWorkTask = _instantiator.Instantiate<HospitalBootstrapTask>(args);
                _bugWorkTask.OnComplete += OnBugWorkTaskEnd;
                _bugWorkTask.OnInterrupt += OnBugWorkTaskEnd;
                _bugWorkTask.OnForceComplete += OnBugWorkTaskEnd;
                _taskStorage.DeclareTask(Id, _selfDto.ModelID, _bugWorkTask.GetName(), _bugWorkTask, false);
            }
        }
        
        private void RepairProduction()
        {
            if (_finalized || !CanProduction())
            {
                foreach (var task in _repairs.Values.ToArray())
                {
                    task?.Interrupt();
                }
                _repairs.Clear();
                return;
            }

            foreach (var slotDto in _hospitalSlotDtoStorage.Get().ToArray())
            {
                if (!_repairs.ContainsKey(slotDto.Id) && slotDto.Repairing)
                {
                    var timerTask = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
                    timerTask.OnComplete += _ => OnRepairCompleteEventHandelr(slotDto.Id);
                    timerTask.SetUpdateAction(left => slotDto.RepairTime.CurrentValue = left);
                    _repairs.Add(slotDto.Id, timerTask); // if simulation Task.Execute will complete called immediately
                    timerTask.Execute(slotDto.RepairTime.CurrentValue);
                }
            }
        }

        private void ReserveProduction()
        {
            if (_finalized)
            {
                foreach (var task in _reserves.Values.ToArray())
                {
                    task?.Interrupt();
                }
                _reserves.Clear();
                return;
            }
            
            foreach (var slotDto in _hospitalSlotDtoStorage.Get().ToArray())
            {
                if (!slotDto.Repairing && !_reserves.ContainsKey(slotDto.Id))
                {
                    var timerTask = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
                    timerTask.OnComplete += _ =>
                    {
                        MessageBroker.Default.Publish(new HospitalRemoveSlotProtocol {SlotId = slotDto.Id});
                    };
                    timerTask.SetUpdateAction(left => slotDto.LifeTime.CurrentValue = left);
                    _reserves.Add(slotDto.Id, timerTask); // if simulation Task.Execute will complete called immediately
                    timerTask.Execute(slotDto.LifeTime.CurrentValue);
                    continue;
                }

                if (slotDto.Repairing && _reserves.ContainsKey(slotDto.Id))
                {
                    var timerTask = _reserves[slotDto.Id];
                    _reserves.Remove(slotDto.Id);
                    timerTask?.Interrupt();
                }
            }
        }

        private void InitBugWorkPauseTimer()
        {
            if (_finalized || !CanProduction())
            {
                return;
            }
            _bugWorkTask?.ForceComplete();
            _bugWorkPauseTimer?.Interrupt();
            _bugWorkPauseTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _bugWorkPauseTimer.OnComplete += _ =>
            {
                _bugWorkPauseTimer = null;
                BugWorkProduction();
            };
            _bugWorkPauseTimer.Execute(Id,_betwenHelpTimeStatKey);
        }

        private void RestoredUnitSpawn(string unitId)
        {
            if (_unitMoverStorage.HasEntity(unitId))
            {
                _instantiator
                    .Instantiate<UnitSpawnCommand<SpawnUnitTask>>()
                    .Execute(new UnitSpawnProtocol(unitId));
            }
        }

        private void DeleteUnit(string unitId)
        {
            if (_unitDtoStorage.HasEntity(unitId))
            {
                _instantiator.Instantiate<DeleteUnitCommand>()
                    .Execute(new DeleteUnitProtocol(unitId));
            }
        }
        
        private void OnBuildCompleted(string buildingId)
        {
            if (_finalized || Id != buildingId)
            {
                return;
            }
            InitBugWorkPauseTimer();
            ReserveProduction();
            RepairProduction();
        }

        private void OnBuildStarted(string buildingId)
        {
            if (_finalized || Id != buildingId)
            {
                return;
            }
            _bugWorkPauseTimer?.Interrupt();
            _bugWorkPauseTimer = null;
            _bugWorkTask?.ForceComplete();
            BugWorkProduction();
            RepairProduction();
        }
        
        private void OnRepairCompleteEventHandelr(string slotId)
        {
            if (!_repairs.ContainsKey(slotId))
            {
                return;
            }
            _repairs.Remove(slotId);
            _hospitalSlotDtoStorage.Remove(slotId);
            RestoredUnitSpawn(slotId);
            MessageBroker.Default.Publish(new EntityRepairedProtocol{EntityId = slotId});
        }
        
        private void OnBugWorkTaskEnd(ITask obj)
        {
            if (_finalized)
            {
                return;
            }

            if (_bugWorkTask != null && _taskStorage.HasTask(_bugWorkTask.Guid))
            {
                _taskStorage.Remove(_bugWorkTask.Guid);
            }
            _bugWorkTask = null;
            _betwenHelpTime?.SetMax();
            InitBugWorkPauseTimer();
        }
        
        private void OnUnitStorageChanged(string unitId)
        {
            if (_finalized)
            {
                return;
            }
            BugWorkProduction();
            ReserveProduction();
            RepairProduction();
        }
        
        private void OnRemoveSlotEventHandler(HospitalRemoveSlotProtocol protocol)
        {
            if (_finalized || !_hospitalSlotDtoStorage.HasEntity(protocol.SlotId))
            {
                return;
            }

            if (_reserves.ContainsKey(protocol.SlotId))
            {
                _reserves[protocol.SlotId]?.Interrupt();
                _reserves.Remove(protocol.SlotId);
            }
            
            _hospitalSlotDtoStorage.Remove(protocol.SlotId);
            MessageBroker.Default.Publish(new DeathUnitProtocol
            {
                DeathReason = DeathReason.Hospital, 
                UnitId = protocol.SlotId
            });
            DeleteUnit(protocol.SlotId);
        }
        
        private void OnRepairSlotEventHandler(HospitalSlotRepairProtocol protocol)
        {
            if (!_hospitalSlotDtoStorage.HasEntity(protocol.SlotId) ||
                _repairs.ContainsKey(protocol.SlotId))
            {
               return;
            }
            
            var slotDto = _hospitalSlotDtoStorage.Get(protocol.SlotId);
            slotDto.Repairing = true;
            ReserveProduction();
            RepairProduction();
        }
        
        private void OnDeathUnitEventHandler(DeathUnitProtocol protocol)
        {
            if (_finalized)
            {
                if (protocol.DeathReason == DeathReason.Fighted)
                {
                    DeleteUnit(protocol.UnitId);
                }
                return;
            }

            if (protocol.DeathReason == DeathReason.Fighted && 
                !_hospitalSlotDtoStorage.HasEntity(protocol.UnitId))
            {
                if (_buildingBuildSystem.IsBuilding(Id))
                {
                    DeleteUnit(protocol.UnitId);
                    return;
                }

                if (!_unitDtoStorage.HasEntity(protocol.UnitId))
                {
                    return;
                }
                
                var unitDto = _unitDtoStorage.Get(protocol.UnitId);
                if (!_hospitalSlotModelStorage.HasEntity(unitDto.ModelID))
                {
                    return;
                }
                var hospitalSlot = new HospitalSlotDto(protocol.UnitId);
                var lifeTime   = _statsCollection.GetValue(_lifeTimeStatKey);
                var repairTime = _statsCollection.GetValue(_repairTimeStatKey);
                hospitalSlot.ModelId = unitDto.ModelID;
                hospitalSlot.LifeTime = new HospitalSlotParam
                {
                    Id = unitDto.Guid,
                    MaxValue = lifeTime,
                    CurrentValue = lifeTime
                };
                hospitalSlot.RepairTime = new HospitalSlotParam
                {
                    Id = unitDto.Guid,
                    MaxValue = repairTime,
                    CurrentValue = repairTime,
                };

                RestoredUnitSpawn(unitDto.Guid);
                _hospitalSlotDtoStorage.Add(hospitalSlot);
                
                _activitySystem.Activate(protocol.UnitId, false);
            }
            ReserveProduction();
            RepairProduction();
        }
        
        private void OnStateInfoUpdate()
        {
            if (_finalized)
            {
                return;
            }

            var bugInfo = "";
            var inside = "";
            var name = LocalizationHelper.GetBugTypeName(_ladyBugModelId);
            if (!CanProduction() && !_buildingBuildSystem.IsBuilding(Id))
            {
                var pattern = LocalizationManager.Localize(_ladyBugNeedTextKey);
                bugInfo = string.Format(pattern, $"<color=red>{name}</color>");
            }

            if (_bugWorkTask != null && _bugWorkTask.IsRunned)
            {
                var pattern = LocalizationManager.Localize(_ladyBugInsideTextKey);
                inside = string.Format(pattern, name);
            }
            _stateInfo.SetInfo(bugInfo, inside);  
        }
    }
}