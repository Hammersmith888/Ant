using Spine;
using Spine.Unity;

namespace BugsFarm.AnimationsSystem
{
    public class SpinePlayer : BasePlayer
    {
        public override float TimeScale
        {
            get => Finalized ? 0 : _skeletonAnimation.timeScale;
            set
            {
                if (Finalized) return;
                _skeletonAnimation.timeScale = value;
            }
        }

        protected override AnimationState AnimationState => _skeletonAnimation.AnimationState;
        private readonly SkeletonAnimation _skeletonAnimation;

        public SpinePlayer(SkeletonAnimation skeletonAnimation)
        {
            _skeletonAnimation = skeletonAnimation;
            _skeletonAnimation.UpdateManually = true;
        }

        protected override void Update(float deltaTime)
        {
            if (Finalized) return;
            _skeletonAnimation.Update(deltaTime);
        }
    }
}