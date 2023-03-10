using Spine.Unity;

namespace BugsFarm.AnimationsSystem
{
    public class SpineAnimator : BaseAnimator
    {
        protected readonly SkeletonAnimation SpineMain;
        protected SpineAnimator(SkeletonAnimation spineMain)
        {
            SpineMain = spineMain;
            SpineMain.Initialize(true);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!Initialized) return;
            SpineMain.AnimationState.Event += SpineAnimationEvent;
        }

        public override void Dispose()
        {
            if (!Initialized) return;
            base.Dispose();
            SpineMain.AnimationState.Event -= SpineAnimationEvent;
        }

        protected override ISpinePlayer CreateBasePlayer()
        {
            return Instantiator.Instantiate<SpinePlayer>(new object[] {SpineMain});
        }
    }
}