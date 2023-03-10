using System;
using Spine;

namespace BugsFarm.AnimationsSystem
{
    public interface ISpinePlayer
    {
        event Action OnComplete;
        float TimeScale { get; set; }
        void Play(Animation animation, bool loop);
    }
}