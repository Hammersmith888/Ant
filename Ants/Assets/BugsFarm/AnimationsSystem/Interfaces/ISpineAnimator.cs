using System;
using BugsFarm.Services.StorageService;
using Spine;
using Zenject;

namespace BugsFarm.AnimationsSystem
{
    public interface ISpineAnimator : IDisposable, IStorageItem, IInitializable
    {
        AnimKey CurrentAnim { get; }
        event Action<AnimKey,Event> OnAnimationEvent;
        event Action<AnimKey> OnAnimationComplete;
        event Action<AnimKey> OnAnimationChanged;
        void SetAnim(AnimKey animation, float timeScaleMul = 1);
        void ChangeModel(AnimationModel newModel);
        bool HasAnim(AnimKey anim);
        float GetDuration(AnimKey anim);
    }
}