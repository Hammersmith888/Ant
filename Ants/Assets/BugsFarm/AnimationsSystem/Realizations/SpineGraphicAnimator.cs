using Spine.Unity;

namespace BugsFarm.AnimationsSystem
{
    public class SpineGraphicAnimator : BaseAnimator
    {
        private readonly SkeletonGraphic _spineMain;

        protected SpineGraphicAnimator(SkeletonGraphic spineMain)
        {
            _spineMain = spineMain;
            _spineMain.Initialize(true);
        }

        public override void Initialize()
        {
            base.Initialize();
            if (!Initialized) return;
            _spineMain.AnimationState.Event += SpineAnimationEvent;
        }

        public override void Dispose()
        {
            if (!Initialized) return;
            base.Dispose();
            _spineMain.AnimationState.Event -= SpineAnimationEvent;
        }

        protected override ISpinePlayer CreateBasePlayer()
        {
            return Instantiator.Instantiate<SpineGraphicPlayer>(new object[] {_spineMain});
        }
    }
}