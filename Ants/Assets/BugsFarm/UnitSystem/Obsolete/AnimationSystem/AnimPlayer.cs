using BugsFarm.SimulationSystem.Obsolete;
using Spine.Unity;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public class AnimPlayer
    {
        public bool Initialized { get; private set; }
        public bool IsAnimComplete => SimulationOld.Type.IsNullOrDefault()? AnimationState.Tracks.Count == 0 || AnimationState.GetCurrent(0).IsComplete : SimulationOld.GameAge > _t_animEnd;
        public float TimeScale
        {
            get => _skeletonAnimation?.timeScale ?? _skeletonGraphic.timeScale;
            set
            {
                if (_skeletonAnimation)
                    _skeletonAnimation.timeScale = value;
                else
                if(_skeletonGraphic)
                    _skeletonGraphic.timeScale = value;
            }
        }
        public string AnimName { get; private set; }

        public Spine.AnimationState AnimationState => _skeletonAnimation?.AnimationState ?? _skeletonGraphic.AnimationState;
        private SkeletonDataAsset SkeletonDataAsset => _skeletonAnimation?.SkeletonDataAsset ?? _skeletonGraphic.SkeletonDataAsset;

        private SkeletonAnimation _skeletonAnimation;
        private SkeletonGraphic _skeletonGraphic;

        private double _t_animEnd;
        public AnimPlayer() { }
        public AnimPlayer(SkeletonAnimation skeletonAnimation)
        {
            _skeletonAnimation = skeletonAnimation;
            Initialized = skeletonAnimation != null;
        }
        public AnimPlayer(SkeletonGraphic skeletonGraphic)
        {
            Init(skeletonGraphic);
        }
        public void Init(SkeletonGraphic skeletonGraphic)
        {
            _skeletonGraphic = skeletonGraphic;
            Initialized = skeletonGraphic != null;
        }
        public void Play(string animationName, bool loop)
        {
            Play(animationName, loop, TimeScale);
        }
        public void Play(string animationName, bool loop, float timeScale)
        {
            AnimName = animationName;
            var duration = SkeletonDataAsset.GetSkeletonData(false).FindAnimation(animationName).Duration;
            _t_animEnd = SimulationOld.GameAge + (timeScale <= 0 ? Mathf.Infinity : duration / timeScale);

            TimeScale = timeScale;
            AnimationState.SetAnimation(0, animationName, loop);
        }
        public void FastForward()
        {
            _skeletonAnimation?.Update(1);
            _skeletonGraphic?.Update(1);
        }
        public void Simulate()
        {
            _skeletonAnimation?.Update(SimulationOld.DeltaTime);
            _skeletonGraphic?.Update(SimulationOld.DeltaTime);
        }
    }
}

