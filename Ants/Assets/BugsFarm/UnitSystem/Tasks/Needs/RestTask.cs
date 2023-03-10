using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using Zenject;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem
{
    public class RestTask : BaseTask
    {
                private readonly float _updateInterval = Random.Range(0.5f,1.5f);
        private PathHelper _pathHelper;
        private IInstantiator _instantiator;
        private ISimulationSystem _simulationSystem;
        private UnitMoverStorage _moverStorage;
        private UnitDtoStorage _unitDtoStorage;
        private AnimatorStorage _animatorStorage;
        private StatsCollectionStorage _statsCollectionStorage;
        private const string _restTimeStatKey = "stat_restTime";
        
        private string _unitId;
        private string _unitModel;
        private IUnitMover _mover;
        private ISpineAnimator _animator;
        private ITask _repositionTask;
        private ITask _updateTask;
        private ITask _restTimer;
        private Action _updateAction;
        private StatVital _restTimeStat;
        [Inject]
        private void Inject(PathHelper pathHelper,
                            IInstantiator instantiator,
                            ISimulationSystem simulationSystem,
                            UnitMoverStorage moverStorage,
                            UnitDtoStorage unitDtoStorage,
                            AnimatorStorage animatorStorage,
                            StatsCollectionStorage statCollectionStorage)
        {
            _pathHelper = pathHelper;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _moverStorage = moverStorage;
            _unitDtoStorage = unitDtoStorage;
            _animatorStorage = animatorStorage;
            _statsCollectionStorage = statCollectionStorage;
        }

        public void SetAction(Action updateAction)
        {
            _updateAction = updateAction;
        }
        
        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            _unitId = (string)args[0];
            var statCollection = _statsCollectionStorage.Get(_unitId);
            var unitDto = _unitDtoStorage.Get(_unitId);
            _unitModel = unitDto.ModelID;
            _restTimeStat = statCollection.Get<StatVital>(_restTimeStatKey);
            _animator = _animatorStorage.Get(_unitId);
            _mover = _moverStorage.Get(_unitId);
            _simulationSystem.OnSimulationEnd += OnSimulationEnd;
            _simulationSystem.OnSimulationStart += OnSimulationStart;
            
            if (_simulationSystem.Simulation)
            {
                OnSimulationStart();
                return;
            }
            StartTask();
            InitState();
        }
        
        private void StartTask()
        {
            _updateTask?.Interrupt();
            var updateTask = _instantiator.Instantiate<SimulatedTimerTask>();
            updateTask.OnComplete += _=> Completed();
            updateTask.SetChunkAction(_updateAction, _updateInterval);
            _updateTask = updateTask;
            _updateTask.Execute(float.MaxValue);
            _updateAction?.Invoke();
        }

        private void InitState()
        {
            if (!IsRunned || IsCompleted)
            {
                return;
            }
            
            if (_restTimeStat.CurrentValue > 0)
            {
                OnDestinationComplete();
            }
            else
            {
                GoRandom();
            }
        }

        private void GoRandom()
        {
            if (!IsRunned || _repositionTask != null || GameReloader.IsReloading) return;

            _restTimer?.Interrupt();
            _restTimer = null;
            _restTimeStat.SetMax();
            
            if (!GetRandomPoint(out var randomNode))
            {
                OnDestinationComplete();
                return;
            }
            var args = new object[] {randomNode.Position};
            _repositionTask = _instantiator.Instantiate<MoveToScenePointTask>(args);
            _repositionTask.OnComplete += _=> OnDestinationComplete();
            _repositionTask.Execute(_unitId);
        }

        private bool GetRandomPoint(out INode node)
        {
            var pathHelperQuery = PathHelperQuery.Empty()
                .UseGraphMask(nameof(RestTask))
                .UseLimitationsCheck(_unitModel)
                .UseTraversableCheck(_mover.TraversableTags);
            var nodes = _pathHelper.GetRandomNodes(pathHelperQuery);
            node = nodes?.FirstOrDefault();
            return node != null;
        }
        
        private void InitRestTimer()
        {
            if(!IsRunned || _restTimer != null) return;
            
            _repositionTask?.Interrupt();
            _repositionTask = null;
            _restTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _restTimer.OnComplete += _ => GoRandom();
            _restTimer.Execute(_unitId,_restTimeStatKey);
        }
        
        private void OnDestinationComplete()
        {
            if (!IsRunned)  return;
            _repositionTask = null;
            _mover.SetLook(Tools.RandomBool());
            _animator.SetAnim(AnimKey.Idle);
            _mover.Stay();
            InitRestTimer();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _restTimeStat.CurrentValue = 0;
                _animator.SetAnim(AnimKey.Idle);
                _mover.Stay();
                _simulationSystem.OnSimulationEnd -= OnSimulationEnd;
                _simulationSystem.OnSimulationStart -= OnSimulationStart;
            }
            
            _restTimeStat = null;
            _restTimer?.Interrupt();
            _restTimer = null;
            _updateTask?.Interrupt();
            _updateTask = null;
            _repositionTask?.Interrupt();
            _repositionTask = null;
            _updateAction = null;
            _unitId = null;
            _mover  = null;
            _animator = null;
            base.OnDisposed();
        }
    #region SimulationHandler
        private void OnSimulationStart()
        {
            if (!IsRunned || IsCompleted || !IsExecuted)
            {
                return;
            }
            _repositionTask?.Interrupt();
            _repositionTask = null;
            _updateTask?.Interrupt();
            _updateTask = null;
            _restTimer?.Interrupt();
            _restTimer = null;
            StartTask();
        }
        
        private void OnSimulationEnd()
        {
            if (!IsRunned || IsCompleted || !IsExecuted || _repositionTask != null)
            {
                return;
            }
            
            if (Tools.RandomBool() && GetRandomPoint(out var randomNode))
            {
                _mover.SetPosition(randomNode);
            }

            StartTask();
            InitState();
        }
    #endregion
    }

    public class PatrolingTask : RestTask
    {
        
    }
}