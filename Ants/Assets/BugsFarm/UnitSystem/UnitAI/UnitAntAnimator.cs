using BugsFarm.AnimationsSystem;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UnitAntAnimator : SpineAnimator
    {
        private SpinePlayer _climbPlayer;
        private readonly SkeletonAnimation _spineClimb;
        public UnitAntAnimator(SkeletonAnimation spineClimb, SkeletonAnimation spineMain) : base(spineMain)
        {
            _spineClimb = spineClimb;
            _spineClimb.Initialize(true);
        }

        protected override ISpinePlayer CreateBasePlayer()
        {
            _climbPlayer = Instantiator.Instantiate<SpinePlayer>(new object[] {_spineClimb});
            return base.CreateBasePlayer();
        }

        public override void Dispose()
        {            
            if(!Initialized) return;
            base.Dispose();
            _climbPlayer.Dispose();
            _climbPlayer = null;
        }

        public override void SetAnim(AnimKey animation, float timeScaleMul = 1)
        {
            if(!Initialized) return;
            if(!_spineClimb || !SpineMain) return;
            
            var isLadder = animation == AnimKey.WalkClimb;
            _spineClimb.gameObject.SetActive(isLadder);
            SpineMain.gameObject.SetActive(!isLadder);
            var player = isLadder ? _climbPlayer : PlayerMain;
            PlayAnim(player, animation, timeScaleMul);
        }
    }
}