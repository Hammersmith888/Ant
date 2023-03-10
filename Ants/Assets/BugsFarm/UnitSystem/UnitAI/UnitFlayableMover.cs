using BugsFarm.AnimationsSystem;

namespace BugsFarm.UnitSystem
{
    public class UnitFlayableMover : UnitMover
    {
        private bool IsFlyable => Current !=null && Current.Tag == 6; // хардкод номера тэга из AstarPath
        private const AnimKey _flyAnimation = AnimKey.Fly;
        private const AnimKey _eatAnimation = AnimKey.Eat;
        private const AnimKey _drinkAnimation = AnimKey.Drink;
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
            if(CantInterruptAnimation()) return;
            _restored = true;
            _interrupted = AnimKey.None;
            OnUpdateVisible();
        }

        protected override void OnUpdateVisible()
        {
            if(IsJoint) return;
            if(IsFlyable && _restored && !CantInterruptAnimation())
            {
                _restored = false;
                _interrupted = Animator.CurrentAnim;
                Animator.SetAnim(_flyAnimation);
            }
            else if(!IsFlyable && !_restored && _interrupted != AnimKey.None)
            {
                Animator.SetAnim(_interrupted);
                _interrupted = AnimKey.None;
                _restored = true;
            }
        }

        private bool CantInterruptAnimation()
        {
            return Animator.CurrentAnim.AnyOff(_flyAnimation, _eatAnimation, _drinkAnimation);
        }
    }
}