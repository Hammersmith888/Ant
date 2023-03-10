using System;
using BugsFarm.TaskSystem;
using Spine;
using UniRx;
using UnityEngine;
using Zenject;
using Event = Spine.Event;

namespace BugsFarm.AnimationsSystem
{
    public abstract class BaseAnimator : ISpineAnimator
    {
        public string Id { get; private set; }
        public AnimKey CurrentAnim { get; private set; }
        public event Action<AnimKey, Event> OnAnimationEvent;
        public event Action<AnimKey> OnAnimationComplete;
        public event Action<AnimKey> OnAnimationChanged;

        private AnimationModel _animationModel;
        protected IInstantiator Instantiator;
        protected ISpinePlayer PlayerMain;
        protected bool Initialized;
        
        [Inject]
        private void Inject(string guid,
                            AnimationModel animationModel,
                            IInstantiator instantiator)
        {
            Id = guid;
            Instantiator = instantiator;
            PlayerMain = CreateBasePlayer();
            ChangeModel(animationModel);
        }

        protected abstract ISpinePlayer CreateBasePlayer();

        public void ChangeModel(AnimationModel newModel)
        {
            _animationModel = newModel;
        }

        public virtual void Initialize()
        {
            if (Initialized) return;
            Initialized = true;
            PlayerMain.OnComplete += AnimationStateOnComplete;
        }

        public virtual void Dispose()
        {
            if (!Initialized) return;
            PlayerMain.OnComplete -= AnimationStateOnComplete;

            if (PlayerMain is IDisposable disposable)
            {
                disposable.Dispose();
            }
            
            _animationModel = null;
            Instantiator = null;
            PlayerMain = null;
            Initialized = false;
            OnAnimationEvent = null;
            OnAnimationComplete = null;
            OnAnimationChanged = null;
        }

        public virtual void SetAnim(AnimKey animation, float timeScaleMul = 1)
        {
            PlayAnim(PlayerMain, animation, timeScaleMul);
        }

        protected virtual void PlayAnim(ISpinePlayer player, AnimKey animation, float timeScaleMul)
        {
            if (!Initialized) return;
            if (!HasAnim(animation))
            {
                Debug.LogWarning($"{this} : {nameof(PlayAnim)} :: Animation with [AnimKey : {animation}], does not exist.");
                CurrentAnim = animation;
                // Will be delayed for avoid infinite loop
                Observable.NextFrame().Subscribe(_ => AnimationComplete(animation));
                return;
            }

            CurrentAnim = animation;
            var animData = _animationModel.GetAnimModel(CurrentAnim);
            var timeScale = animData.TimeScale * timeScaleMul;

            player.TimeScale = timeScale;
            player.Play(animData.ReferenceAsset.Animation, animData.IsLoop);
            AnimationChanged(animation);
        }

        public bool HasAnim(AnimKey anim)
        {
            return Initialized && _animationModel.HasAnim(anim);
        }

        public float GetDuration(AnimKey anim)
        {
            return HasAnim(anim)
                ? _animationModel.GetAnimModel(anim).ReferenceAsset.Animation.Duration / PlayerMain.TimeScale
                : 0;
        }

        private void AnimationStateOnComplete()
        {
            if (!Initialized) return;
            AnimationComplete(CurrentAnim);
        }

        protected void SpineAnimationEvent(TrackEntry trackEntry, Event e)
        {
            if (!Initialized) return;
            OnAnimationEvent?.Invoke(CurrentAnim, e);
        }

        protected void AnimationChanged(AnimKey anim)
        {
            if (!Initialized) return;
            OnAnimationChanged?.Invoke(anim);
        }

        protected void AnimationComplete(AnimKey anim)
        {
            if (!Initialized) return;
            OnAnimationComplete?.Invoke(anim);
        }
    }
}