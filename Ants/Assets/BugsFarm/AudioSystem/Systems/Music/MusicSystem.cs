using System;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.SaveManagerService;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.AudioSystem
{
    public class MusicSystem : IMusicSystem, IInitializable, IDisposable
    {
        public bool Mute { get; private set; }
        public float Volume { get; private set; } = 1f;

        private readonly IInstantiator _instantiator;
        private readonly ISaveManagerLocal _saveManagerLocal;
        private IMusicPlayer _current;
        
        public MusicSystem(IInstantiator instantiator,
                           ISaveManagerLocal saveManagerLocal)
        {
            _instantiator = instantiator;
            _saveManagerLocal = saveManagerLocal;
            MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloads);
        }
        
        public void PlayTransition(string musicPath, float duration = 1f, PlayMod playMod = PlayMod.CrossFade)
        {
            StopWithFading(duration);
            Play(musicPath, playMod);
        }
        
        public void Play(string musicPath, PlayMod playMod = PlayMod.CrossFade)
        {
            switch (playMod)
            {
                case PlayMod.CrossFade:
                case PlayMod.Once:
                case PlayMod.Loop: 
                default: Play<CrossFadePlayer>(musicPath); break;
            }
        }

        public void Play<T>(string musicPath) where T : IMusicPlayer
        {
            _current?.Dispose();
            _current = _instantiator.Instantiate<T>();
            _current.SetVolume(Volume);
            _current.SetMute(Mute);
            _current.Play(musicPath);
        }

        public void Stop()
        {
            _current?.Dispose();
            _current = null;
        }
        
        public void StopWithFading(float duration = 1f)
        {
            if (_current == null)
            {
                return;
            }
            
            var lastPlayer = _current;
            lastPlayer.VolumeTransition(0f, duration, lastPlayer.Dispose);
            _current = null;
        }

        public void SetMute(bool value)
        {
            Mute = value;
            _current?.SetMute(value);
        }

        public void SetVolume(float value)
        {
            Volume = value;
            _current?.SetVolume(value);
        }

        private void OnGameReloads(GameReloadingReport report)
        {
            Stop();
        }
    #region Save

        [Serializable]
        private struct MusicData
        {
            public bool Mute;
            public float Volume;
        }

        private const string _saveKey = "musicData";

        void IInitializable.Initialize()
        {
            if (_saveManagerLocal.HasSaves(_saveKey))
            {
                var jsonData = _saveManagerLocal.Load(_saveKey);
                var data = JsonUtility.FromJson<MusicData>(jsonData);
                SetMute(data.Mute);
                SetVolume(data.Volume);
            }
            else
            {
                SetMute(false);
                SetVolume(1f);
            }
        }

        void IDisposable.Dispose()
        {
            var data = JsonUtility.ToJson(new MusicData {Mute = Mute, Volume = 1f});
            _saveManagerLocal.Save(_saveKey, data);
        }
    #endregion
    }
}