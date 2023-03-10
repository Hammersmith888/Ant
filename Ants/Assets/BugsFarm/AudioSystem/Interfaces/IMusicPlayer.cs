using System;

namespace BugsFarm.AudioSystem
{
    public interface IMusicPlayer : IDisposable
    {
        void Play(string clipPath);

        void Stop();
        
        void SetVolume(float value);

        void SetMute(bool value);

        void VolumeTransition(float volume, float duration = 1.5f, Action onComplete = null);
    }
}