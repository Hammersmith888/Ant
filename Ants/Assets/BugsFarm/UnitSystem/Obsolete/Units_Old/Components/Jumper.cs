using System;
using BugsFarm.AnimationsSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public enum JumpState
    {
        None,

        Jump,
        Fall,
        Land
    }

    [Serializable]
    public class Jumper
    {
        public bool IsJumping => _state != JumpState.None;

        [NonSerialized] Transform _agent;
        [NonSerialized] AntAnimator _antAnimator;

        private const float Ref_Height = 2.138f;
        private const float Ref_Time = .5f;
        private readonly Timer _timer = new Timer();
        private JumpState _state;
        private SVec2 _begin;
        private SVec2 _tgt;
        private float _flyTime;

        public void Init(Transform agent, AntAnimator antAnimator)
        {
            _agent = agent;
            _antAnimator = antAnimator;
        }
        public void Jump(GStep gStep)
        {
            _begin = gStep.PointAt_t0;
            _tgt = gStep.PointAt_t1;

            float h = gStep.PointAt_t0.y - gStep.PointAt_t1.y;
            _flyTime = Ref_Time * Mathf.Sqrt(h / Ref_Height);

            Transition(JumpState.Jump);
        }
        public void Update()
        {
            switch (_state)
            {
                case JumpState.Jump:
                    if (_antAnimator.IsAnimComplete)
                    {
                        _timer.Set(_flyTime);

                        Transition(JumpState.Fall);
                    }
                    break;

                case JumpState.Fall:
                    float t = Mathf.Pow(_timer.Progress, 2);
                    _agent.position = _agent.position.SetXY(Vector2.Lerp(_begin, _tgt, t));

                    if (_timer.IsReady)
                        Transition(JumpState.Land);
                    break;

                case JumpState.Land:
                    if (_antAnimator.IsAnimComplete)
                        Transition(JumpState.None);
                    break;
            }
        }

        private void Transition(JumpState state)
        {
            _state = state;

            SetAnim();
        }
        private void SetAnim()
        {
            switch (_state)
            {
                case JumpState.Jump: _antAnimator.SetAnim(AnimKey.JumpOff); break;
                case JumpState.Fall: _antAnimator.SetAnim(AnimKey.JumpFall); break;
                case JumpState.Land: _antAnimator.SetAnim(AnimKey.JumpLand); break;

                //case JumpState.None: _antAnimator.SetAnim(_antAnimator.WalkAnim); break;
            }
        }
    }
}