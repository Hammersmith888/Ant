using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Tweens
{
    public class TweenMover2D : UITweenMover
    {
        [SerializeField] private bool _safeZDepth = true;
        public Vector2 CurrentState => transform.position;
        public Vector2 EndState { get => _endState; set => _endState = value; }
        public Vector2 StartState { get => _startState; set => _startState = value; }
        protected override void DoReset(Vector2 toState)
        {
            Init();
            Vector3 statePos = toState;
            if (_safeZDepth)
            {
                statePos.z = transform.position.z;
            }
            transform.position = statePos;
        }
        protected override void JoinX(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(transform.DOMoveX(state, duration));
        }
        protected override void JoinY(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(transform.DOMoveY(state, duration));
        }
        protected override void SetStartState()
        {
            _startState = transform.position;
        }
        protected override void SetEndState()
        {
            _endState = transform.position;
        }
    }
}