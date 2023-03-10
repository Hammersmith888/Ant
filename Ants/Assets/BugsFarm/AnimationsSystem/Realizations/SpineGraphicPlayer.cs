using Spine;
using Spine.Unity;

namespace BugsFarm.AnimationsSystem
{
    public class SpineGraphicPlayer : BasePlayer
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
        private readonly SkeletonGraphic _skeletonAnimation;

        public SpineGraphicPlayer(SkeletonGraphic skeletonAnimation)
        {
            _skeletonAnimation = skeletonAnimation;
            _skeletonAnimation.UpdateManually = true;
        }

        protected override void Update(float deltaTime)
        {
            if(Finalized) return;
            _skeletonAnimation.Update(deltaTime);
        }
    }
}