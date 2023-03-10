using System;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using UniRx;
using Zenject;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseTrainingTask : BaseTask
    {
        private int XpDuration { get; } = 60; // seconds
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private IInstantiator _instantiator;
        protected ISimulationSystem SimulationSystem;

        private const string _unitExpStatKey = "stat_xp";
        private const string _trainTaskTimeStatKey = "stat_trainTaskTime";
        private const string _experienceStatKey = "stat_experience";
        private const string _restTimeMinStatKey = "stat_restTimeMin";
        private const string _restTimeMaxStatKey = "stat_restTimeMax";
        private const string _trainTimeMinStatKey = "stat_trainTimeMin";
        private const string _trainTimeMaxStatKey = "stat_trainTimeMax";
        
        private StatVital _unitExpStat;
        private StatsCollection _statsCollection;
        protected BuildingSceneObject SceneObject;

        private float _targetExpStep;
        private float _trainSecondTime;
        private float _restSecondsTime;
        
        private ITask _taskTimer;
        private ITask _trainTimer;
        private ITask _restTimer;
        private IDisposable _moveBuildingEvent;

        [Inject]
        private void Inject(StatsCollectionStorage statCollectionStorage, 
                            IInstantiator instantiator,
                            ISimulationSystem simulationSystem,
                            BuildingSceneObjectStorage buildingSceneObjectStorage)
        {
            _statsCollectionStorage = statCollectionStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _instantiator = instantiator;
            SimulationSystem = simulationSystem;
        }

        public sealed override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);
            var unitGuid = (string) args[0];
            var buildingGuid = (string) args[1];
            var unitStatCollection = _statsCollectionStorage.Get(unitGuid);
            SceneObject = _buildingSceneObjectStorage.Get(buildingGuid);
            _moveBuildingEvent = MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(protocol =>
            {
                if (protocol.Guid == buildingGuid)
                {
                    Interrupt();
                }
            });
            _statsCollection = _statsCollectionStorage.Get(buildingGuid);
            _unitExpStat = unitStatCollection.Get<StatVital>(_unitExpStatKey);
            
            _trainSecondTime = Random.Range(_statsCollection.GetValue(_trainTimeMinStatKey), 
                                      _statsCollection.GetValue(_trainTimeMaxStatKey));
            
            _restSecondsTime  = Random.Range(_statsCollection.GetValue(_restTimeMinStatKey), 
                                      _statsCollection.GetValue(_restTimeMaxStatKey));
            
            _targetExpStep = _statsCollection.GetValue(_experienceStatKey) /
                             _statsCollection.GetValue(_trainTaskTimeStatKey);
            ExecuteInheritor(args);
            SimulationSystem.OnSimulationStart += OnSimulationStart;
            SimulationSystem.OnSimulationEnd += OnSimulationEnded;
            InitTaskTimer();
            if (!SimulationSystem.Simulation)
            {
                InitTrainTimer();
            }
        }

        private void AddXp()
        {
            if(!IsRunned || IsCompleted || _taskTimer == null) return;

            // выход если достигли предела опыта но,
            // пока что нет проверок по уровню - задачи зацикливаются, по этому коммент кода.

            // var summ = _unitExpStat.CurrentValue + _targetExpStep;
            // if (summ >= _unitExpStat.BaseValue)
            // {
            //     Transition(TrainState.None);
            // }
            //  _unitExpStat.CurrentValue += summ;

            // временно продолжают получать опыт и задача не заканчивается по достижению максимума
            _unitExpStat.CurrentValue += _targetExpStep;
        }

        private void InitTaskTimer()
        {
            if(!IsRunned || IsCompleted || _taskTimer != null) return;
            var taskTimer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
            taskTimer.OnComplete += _ => Completed();
            taskTimer.SetChunkAction(AddXp, XpDuration);
            _taskTimer = taskTimer;
            _taskTimer.Execute(_statsCollection.GetValue(_trainTaskTimeStatKey));
        }
        
        private void InitTrainTimer()
        {
            if(!IsRunned || IsCompleted) return;
            _restTimer?.Interrupt();
            _restTimer = null;
            
            _trainTimer = _instantiator.Instantiate<SimulatedTimerTask>();
            _trainTimer.OnComplete += _ => OnTrainEnd();
            _trainTimer.Execute(_trainSecondTime);
            OnTrain();
        }        
        
        private void InitRestTimer()
        {
            if(!IsRunned || IsCompleted) return;
            _trainTimer?.Interrupt();
            _trainTimer = null;
            
            _restTimer = _instantiator.Instantiate<SimulatedTimerTask>();
            _restTimer.OnComplete      += _ => OnRestEnd();
            _restTimer.Execute(_restSecondsTime);
            OnRest();
        }

        private void OnTrainEnd()
        {
            if(!IsRunned || IsCompleted) return;
            _trainTimer?.Interrupt();
            _trainTimer = null;
            InitRestTimer();
        }
        
        private void OnRestEnd()
        {
            if(!IsRunned || IsCompleted || _trainTimer != null) return;
            _restTimer?.Interrupt();
            _restTimer = null;
            InitTrainTimer();
        }

        protected abstract void OnTrain();

        protected abstract void OnRest();
        
        protected virtual void ExecuteInheritor(params object[] args)
        {
            
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                SimulationSystem.OnSimulationStart -= OnSimulationStart;
                SimulationSystem.OnSimulationEnd -= OnSimulationEnded;
            }
            _taskTimer?.Interrupt();
            _taskTimer = null;

            _trainTimer?.Interrupt();
            _trainTimer = null;
            
            _restTimer?.Interrupt();
            _restTimer = null;
            
            _unitExpStat = null;
            _moveBuildingEvent?.Dispose();
            _moveBuildingEvent = null;
            SceneObject = null;
        }

        private void OnSimulationEnded()
        {
            if(!IsRunned || IsCompleted || _taskTimer == null) return;
            InitTrainTimer();
        }

        private void OnSimulationStart()
        {
            if(!IsRunned || IsCompleted) return;
            _trainTimer?.Interrupt();
            _trainTimer = null;
            
            _restTimer?.Interrupt();
            _restTimer = null;
            
            InitTaskTimer();
        }
    }
}