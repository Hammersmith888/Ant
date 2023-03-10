namespace BugsFarm.AudioSystem
{
    public interface IMusicSystem
    {
        bool Mute { get; }

        float Volume { get; }

        void PlayTransition(string musicPath, float duration = 1f, PlayMod playMod = PlayMod.CrossFade);

        void Play(string musicPath, PlayMod playMod = PlayMod.CrossFade);

        void Play<T>(string musicPath) where T : IMusicPlayer;

        void Stop();

        void StopWithFading(float duration = 1f);

        void SetMute(bool value);

        void SetVolume(float value);
    }
}