using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class MoveToScenePointTask : BaseTask
    {
        private readonly AnimKey _walkAnim;
        private readonly Vector2 _point;
        private readonly IPointGraph _pointGraph;
        private readonly UnitMoverStorage _moverStorage;
        private readonly AnimatorStorage _animatorStorage;

        private IUnitMover _mover;
        private ISpineAnimator _animator;

        public MoveToScenePointTask(Vector2 point,
                                    IPointGraph pointGraph,
                                    UnitMoverStorage moverStorage,
                                    AnimatorStorage animatorStorage,
                                    AnimKey walkAnim = AnimKey.Walk)
        {
            _point = point;
            _pointGraph = pointGraph;
            _moverStorage = moverStorage;
            _animatorStorage = animatorStorage;
            _walkAnim = walkAnim;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            var unitGuid = (string) args[0];


            _animator = _animatorStorage.Get(unitGuid);
            _mover = _moverStorage.Get(unitGuid);
            _mover.OnComplete += OnDestinationComplete;
            _pointGraph.OnUpdate += MoveToTarget;
            MoveToTarget();
        }

        private void MoveToTarget()
        {
            if (!IsExecuted || !IsRunned || IsCompleted)
            {
                return;
            }
            _animator.SetAnim(_walkAnim);
            _mover.GoTarget(_point);
        }

        public override Vector2[] GetPositions()
        {
            return new[] {_point};
        }

        private void OnDestinationComplete()
        {
            if (!IsRunned || !IsExecuted)
            {
                return;
            }

            Completed();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _pointGraph.OnUpdate -= MoveToTarget;
                _mover.OnComplete -= OnDestinationComplete;
                _mover.Stay();
            }

            _mover = null;
            _animator = null;
        }

        protected override void OnForceCompleted()
        {
            if (IsExecuted)
            {
                _mover?.ReachTarget();
                _mover?.SetPosition(_point);
            }
            base.OnForceCompleted();
        }
    }
}