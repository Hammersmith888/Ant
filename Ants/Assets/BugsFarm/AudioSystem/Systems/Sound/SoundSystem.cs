using System;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.SaveManagerService;
using UnityEngine;
using Zenject;

namespace BugsFarm.AudioSystem
{
    public class SoundSystem : ISoundSystem, IInitializable, IDisposable
    {
        public bool Mute { get; private set; }
        public float Volume { get; private set; }
        
        private readonly SoundPlayer _soundPlayer;
        private readonly SoundLoader _soundLoader;
        private readonly ISaveManagerLocal _saveManagerLocal;

        public SoundSystem(SoundPlayer soundPlayer,
                           SoundLoader soundLoader,
                           ISaveManagerLocal saveManagerLocal)
        {
            _soundPlayer = soundPlayer;
            _soundLoader = soundLoader;
            _saveManagerLocal = saveManagerLocal;
        }
        
        public void Play(Vector2 worldPosition, 
                         string clipPath, 
                         float volume = 1f)
        {
            if (_soundPlayer.PlayerWhole.mute)
            {
                return;
            }

            if (!_soundLoader.TryLoad(clipPath, out var clip, true))
            {
                return;
            }

            var player = ByPosition(worldPosition);
            player.volume = volume;
            player.PlayOneShot(clip);
        }
    
        public void Play(string clipPath, float volume = 1f)
        {
            if (_soundPlayer.PlayerWhole.mute)
            {
                return;
            }

            if (!_soundLoader.TryLoad(clipPath, out var clip, true))
            {
                return;
            }

            _soundPlayer.PlayerWhole.volume = volume;
            _soundPlayer.PlayerWhole.PlayOneShot(clip);
        }

        public void SetMute(bool value)
        {
            foreach (var player in _soundPlayer.Players)
            {
                player.mute = value;
            }

            _soundPlayer.PlayerWhole.mute = value;
            Mute = value;
        }

        public void SetVolume(float value)
        {
            foreach (var player in _soundPlayer.Players)
            {
                player.volume = value;
            }

            _soundPlayer.PlayerWhole.volume = value;
            Volume = value;
        }
        
        private AudioSource ByPosition(Vector2 worldSpace)
        {
            return _soundPlayer
                .Players
                .OrderBy(player => ((Vector2)player.transform.position - worldSpace).magnitude)
                .First();
        }
        
    #region Savable

        [Serializable]
        private struct SoundData
        {
            public bool Mute;
            public float Volume;
        }

        private const string _saveKey = "soundData";

        void IInitializable.Initialize()
        {
            if (_saveManagerLocal.HasSaves(_saveKey))
            {
                var jsonData = _saveManagerLocal.Load(_saveKey);
                var data = JsonUtility.FromJson<SoundData>(jsonData);
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
            var data = JsonUtility.ToJson(new SoundData {Mute = Mute, Volume = 1f});
            _saveManagerLocal.Save(_saveKey, data);
        }

    #endregion
    }
}