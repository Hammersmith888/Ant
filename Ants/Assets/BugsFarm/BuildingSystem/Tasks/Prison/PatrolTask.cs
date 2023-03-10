using System;
using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class PatrolTask : BaseTask
    {
        private readonly string _buildingId;
        private readonly IInstantiator _instantiator;
        private readonly UnitMoverStorage _moverStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly PointsController _pointsController;
        private const string _patrolTimeStatKey = "stat_patrolTime";
        private const string _patrolRepositionTimeStatKey = "stat_patrolRepositionTime";
        private float _patrolRepositionTime;
        private string _unitId;
        private IUnitMover _mover;
        private ISpineAnimator _animator;
        private ITask _patrolTask;
        private ITask _taskTimer;
        private ITask _repositionTask;
        private IDisposable _moveEvent;

        public PatrolTask(string buildingId,
                          IEnumerable<IPosSide> taskPoints,
                          IInstantiator instantiator,
                          UnitMoverStorage moverStorage,
                          AnimatorStorage animatorStorage,
                          StatsCollectionStorage statsCollectionStorage)
        {
            _buildingId = buildingId;
            _instantiator = instantiator;
            _moverStorage = moverStorage;
            _animatorStorage = animatorStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _pointsController = new PointsController();
            _pointsController.Initialize(1, taskPoints);
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            _unitId = (string) args[0];
            if (!_animatorStorage.HasEntity(_unitId))
            {
                throw new InvalidOperationException();
            }

            if (!_moverStorage.HasEntity(_unitId))
            {
                throw new InvalidOperationException();
            }

            if (!_statsCollectionStorage.HasEntity(_buildingId))
            {
                throw new InvalidOperationException();
            }

            var statCollection = _statsCollectionStorage.Get(_buildingId);
            _patrolRepositionTime = statCollection.GetValue(_patrolRepositionTimeStatKey);
            _taskTimer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
            _taskTimer.OnComplete += _ => Completed();
            _taskTimer.Execute(statCollection.GetValue(_patrolTimeStatKey));
            
            if (_taskTimer.IsCompleted)
            {
                return;
            }
            
            _moveEvent = MessageBroker.Default
                .Receive<PlaceBuildingCommand>()
                .Subscribe(OnMoveBuildingEventHandler);
            
            _animator = _animatorStorage.Get(_unitId);
            _mover = _moverStorage.Get(_unitId);
            _pointsController.GetPoint();
            OnDestinationComplete();
        }

        private void Reposition()
        {
            if (!IsRunned || _repositionTask != null)
            {
                return;
            }
            _patrolTask?.Interrupt();
            _patrolTask = null;
            var args = new object[] {_buildingId, _pointsController.GetPoint()};
            _repositionTask = _instantiator.Instantiate<MoveToSceneItemTask>(args);
            _repositionTask.OnComplete += _ => OnDestinationComplete();
            _repositionTask.Execute(_unitId);
        }

        private void InitRepositionTimer()
        {
            _patrolTask?.Interrupt();
            _patrolTask = _instantiator.Instantiate<SimulatedTimerTask>();
            _patrolTask.OnComplete += _ => Reposition();
            _patrolTask.Execute(_patrolRepositionTime);
        }

        private void OnDestinationComplete()
        {
            if (!IsRunned) return;
            _repositionTask = null;
            _animator.SetAnim(AnimKey.Idle);
            _pointsController.FreePoint();
            InitRepositionTimer();
        }
        
        private void OnMoveBuildingEventHandler(PlaceBuildingCommand protocol)
        {
            if (!IsRunned || _repositionTask != null)
            {
                return;
            }
            
            Reposition();
        }

        protected override void OnDisposed()
        {
            _moveEvent?.Dispose();
            _mover?.Stay();
            _animator?.SetAnim(AnimKey.Idle);
            _patrolTask?.Interrupt();
            _taskTimer?.Interrupt();
            _repositionTask?.Interrupt();
            _pointsController.Dispose();
            _moveEvent = null;
            _patrolTask = null;
            _repositionTask = null;
            _taskTimer = null;
            _unitId = null;
            _mover = null;
            _animator = null;
            base.OnDisposed();
        }
    }
}