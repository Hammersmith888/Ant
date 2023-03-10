using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Tweens
{
    public class UITweenScaler : UITweenMover
    {
        protected override void DoReset(Vector2 toState)
        {
            DoReset();
            GetComponent<RectTransform>().localScale = toState;
        }
        protected override void JoinX(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(GetComponent<RectTransform>().DOScaleX(state, duration));
        }
        protected override void JoinY(float state, float duration)
        {
            if (Sequence.IsNullOrDefault()) return;
            Sequence.Join(GetComponent<RectTransform>().DOScaleY(state, duration));
        }
        protected override void SetStartState()
        {
            _startState = GetComponent<RectTransform>().localScale;
        }
        protected override void SetEndState()
        {
            _endState = GetComponent<RectTransform>().localScale;
        }

    }
}