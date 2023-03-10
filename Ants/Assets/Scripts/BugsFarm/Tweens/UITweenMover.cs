using System;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Tweens
{
    public class UITweenMover : Tween
    {
        [Header("Use context menu for setup state")]
        #region SerializedFields
        [Tooltip("Delayed play the animation in seconds")]
        [SerializeField] protected float _delayedForwardPlay = 0f;

        [Tooltip("Delayed rewind the animation in seconds")]
        [SerializeField] protected float _delayedRewindPlay = 0f;

        [Tooltip("Initial state")]
        [SerializeField] protected Vector2 _startState = Vector2.zero;

        [Tooltip("Target state animation")]
        [SerializeField] protected Vector2 _endState = Vector2.zero;

        [Tooltip("Duration - speed of the forward animation")]
        [SerializeField] protected Vector2 _forwardDuration = Vector2.one;

        [Tooltip("Duration - speed of the rewind animation")]
        [SerializeField] protected Vector2 _rewindDuration = Vector2.one;

        [Tooltip("Ease animation type on Play")]
        [SerializeField] protected Ease _playEase = Ease.InOutQuad;

        [Tooltip("Ease animation type on Rewind")]
        [SerializeField] protected Ease _rewindEase = Ease.InOutQuad;

        [Tooltip("Selects the direction of the animation")]
        [SerializeField] protected Direction _direction = Direction.Xy;
        #endregion SerializedFields

        public bool IsComplete { get; private set; } = true;
        protected enum Direction { X, Y, Xy }
        public override void Do(Action onComplete = null)
        {
            IsComplete = false;
            DoReset(_startState);
            SetupSequence(_playEase, _endState, _forwardDuration, _delayedForwardPlay, onComplete);
        }
        public override void Rewind(Action onComplete = null)
        {
            IsComplete = false;
            DoReset(_endState);
            SetupSequence(_rewindEase, _startState, _rewindDuration, _delayedRewindPlay, onComplete);
        }
        public override void DoReset()
        {
            GetComponent<RectTransform>().DOKill();
            Init();
        }
        protected virtual void DoReset(Vector2 toState)
        {
            DoReset();
            GetComponent<RectTransform>().anchoredPosition = toState;
        }

        private void SetupSequence(Ease currentAnim
                                 , Vector2 toState
                                 , Vector2 duration
                                 , float delay = 0
                                 , Action onComplete = null)
        {
            Sequence.Pause();
            Sequence.SetAutoKill(false);
            Sequence.SetEase(currentAnim);
            SetupAnimation(toState, duration);
            Sequence.OnComplete(() => { onComplete?.Invoke(); Sequence.Kill(); IsComplete = true;});
            DOVirtual.DelayedCall(delay, () => Sequence.Restart());
        }

        private void SetupAnimation(Vector2 state, Vector2 duration)
        {
            switch (_direction)
            {
                case Direction.X:
                    JoinX(state.x, duration.x);
                    break;
                case Direction.Y:
                    JoinY(state.y, duration.y);
                    break;
                case Direction.Xy:
                    JoinX(state.x, duration.x); JoinY(state.y, duration.y);
                    break;
                default: throw new ArgumentOutOfRangeException();
            }
        }
        protected virtual void JoinX(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(GetComponent<RectTransform>().DOAnchorPosX(state, duration));
        }
        protected virtual void JoinY(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(GetComponent<RectTransform>().DOAnchorPosY(state, duration));
        }
        
        [ExposeMethodInEditor]
        protected virtual void SetStartState()
        {
            _startState = GetComponent<RectTransform>().anchoredPosition;
        }
        [ExposeMethodInEditor]
        protected virtual void SetEndState()
        {
            _endState = GetComponent<RectTransform>().anchoredPosition;
        }
    }
}