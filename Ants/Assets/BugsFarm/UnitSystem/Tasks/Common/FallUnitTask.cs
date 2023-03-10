using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.Graphic;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class FallUnitTask : BaseTask, ITickable
    {
        public override bool Interruptible => false;
        private readonly UnitMoverStorage _moverStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly ITickableManager _tickableManager;
        private readonly PathHelper _pathHelper;
        private float Gravity => -Physics2D.gravity.y;
        private bool _initialized;
        private IUnitMover _mover;
        private ISpineAnimator _animator;
        private INode _targetPosition;
        private const float _distanceTreshold = 0.1f;
        private const AnimKey _fallAnimkey = AnimKey.JumpFall;
        private const AnimKey _fallEndAnimkey = AnimKey.JumpLand;

        public FallUnitTask(UnitMoverStorage moverStorage,
                            UnitDtoStorage unitDtoStorage,
                            AnimatorStorage animatorStorage,
                            UnitSceneObjectStorage unitSceneObjectStorage,
                            ISimulationSystem simulationSystem,
                            ITickableManager tickableManager,
                            PathHelper pathHelper)
        {
            _moverStorage = moverStorage;
            _unitDtoStorage = unitDtoStorage;
            _animatorStorage = animatorStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
            _simulationSystem = simulationSystem;
            _tickableManager = tickableManager;
            _pathHelper = pathHelper;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);

            var unitGuid = (string) args[0];
            var unitView = _unitSceneObjectStorage.Get(unitGuid);
            var unitDto = _unitDtoStorage.Get(unitGuid);
            _mover = _moverStorage.Get(unitGuid);
            _animator = _animatorStorage.Get(unitGuid);
            var findQuery = PathHelperQuery.Empty(_mover.Position)
                .UseTraversableCheck(_mover.TraversableTags)
                .UseLimitationsCheck(unitDto.ModelID)
                .ProjectPosition();
            _targetPosition = _pathHelper.FallRayCast(findQuery);

            if (_targetPosition == null)
            {
                Interrupt();
                return;
            }
            
            unitView.SetLayer(new LocationLayer(_targetPosition.LayerName, 0));
            _tickableManager.Add(this);
            _initialized = true;
            _animator.SetAnim(_fallAnimkey);
        }

        public void Tick()
        {
            if (!IsRunned) return;
            
            if (_simulationSystem.Simulation)
            {
                _mover.SetPosition(_targetPosition.Position);
                _animator.SetAnim(_fallEndAnimkey);
                Completed();
                return;
            }
            
            if ((_mover.Position - _targetPosition.Position).magnitude > _distanceTreshold)
            {
                _mover.SetPosition(Vector2.MoveTowards(_mover.Position, _targetPosition.Position,
                                                       _simulationSystem.DeltaTime * Gravity));
            }
            else
            {
                _animator.SetAnim(_fallEndAnimkey);
                Completed();
            }
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _mover.SetPosition(_targetPosition);
                _mover.Stay();
                if (_initialized)
                {
                  _tickableManager.Remove(this);  
                }
            }

            _mover = null;
            _animator = null;
            _targetPosition = null;
            base.OnDisposed();
        }
    }
}