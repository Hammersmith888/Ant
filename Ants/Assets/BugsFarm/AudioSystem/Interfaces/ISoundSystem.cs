using UnityEngine;

namespace BugsFarm.AudioSystem
{
    public interface ISoundSystem
    {
        bool Mute { get; }
        float Volume { get; }
        
        void Play(Vector2 worldPosition, string clipPath, float volume = 1f);
        void Play(string clipPath, float volume = 1f);
        void SetMute(bool value);
        
        void SetVolume(float value);
    }
}