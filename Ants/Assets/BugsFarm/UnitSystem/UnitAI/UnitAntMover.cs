using BugsFarm.AnimationsSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UnitAntMover : UnitMover
    {
        public bool IsLadder => Current != null && Current.Tag == 2; // веменно хардкод номера тэга из AstarPath
        private const AnimKey _climbAnimation = AnimKey.WalkClimb;
        private AnimKey _interrupted = AnimKey.None;
        private bool _restored = true;
        
        protected override void OnInitialized()
        {
            Animator.OnAnimationChanged += OnAnimationChanged;
            base.OnInitialized();
        }
        
        protected override void OnDisposed()
        {
            Animator.OnAnimationChanged -= OnAnimationChanged;
            base.OnDisposed();
        }
        
        private void OnAnimationChanged(AnimKey anim)
        {
            if(anim == _climbAnimation) return;
            _restored = true;
            _interrupted = AnimKey.None;
            OnUpdateVisible();
        }
        protected override Quaternion GetRotationToPath(out bool forceRotate, bool? overrideLookLeft = null)
        {
            if (IsLadder)
            {
                forceRotate = false;
                return Transform.rotation;
            }
            return base.GetRotationToPath(out forceRotate, overrideLookLeft);
        }

        protected override void OnUpdateVisible()
        {
            if(IsJoint || SimulationSystem.Simulation) return;
            if(IsLadder && _restored)
            {
                _restored = false;
                _interrupted = Animator.CurrentAnim;
                Animator.SetAnim(_climbAnimation);
            }
            else if(!IsLadder && !_restored && _interrupted != AnimKey.None)
            {
                Animator.SetAnim(_interrupted);
                _interrupted = AnimKey.None;
                _restored = true;
            }
        }
    }
}