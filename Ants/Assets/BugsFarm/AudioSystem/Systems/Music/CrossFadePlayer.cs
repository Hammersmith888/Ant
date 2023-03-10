using System;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.MonoPoolService;
using DG.Tweening;
using UnityEngine;
using Zenject;

namespace BugsFarm.AudioSystem
{
    public class CrossFadePlayer : IMusicPlayer
    {
        private AudioPlayer Current => _pingPong ? _source1 : _source2;
        private readonly IMonoPool _monoPool;
        private readonly IInstantiator _instantiator;
        private readonly SoundLoader _soundLoader;
        private readonly float _overlapTime;
        
        private Tween _overlapTransition;
        private Tween _volumeTransition;
        private AudioPlayer _source1;
        private AudioPlayer _source2;

        private AudioClip _clip;
        private bool _pingPong;
        
        public CrossFadePlayer(IMonoPool monoPool,
                               IInstantiator instantiator,
                               SoundLoader soundLoader,
                               bool audio2D = true,
                               float overlapTime = 2.5f,
                               float minDistance = 50f,
                               float maxDistance = 100f)
        {

            _monoPool = monoPool;
            _instantiator = instantiator;
            _soundLoader = soundLoader;
            _source1 = monoPool.Spawn<AudioPlayer>();
            _source2 = monoPool.Spawn<AudioPlayer>();
            _overlapTime = overlapTime;
            _source1.Player.minDistance = minDistance;
            _source2.Player.minDistance = minDistance;
            _source1.Player.maxDistance = maxDistance;
            _source2.Player.maxDistance = maxDistance;
            _source1.Player.spatialBlend = audio2D ? 0 : 1;
            _source2.Player.spatialBlend = audio2D ? 0 : 1;
        }

        public void Dispose()
        {
            if (NotAvailable())
            {
                return;
            }

            Stop();
            _monoPool.Despawn(_source1);
            _monoPool.Despawn(_source2);
            _source1 = _source2 = null;
            _clip = null;
        }

        public void Play(string clipPath)
        {
            if (!_soundLoader.TryLoad(clipPath, out var clip, true) || NotAvailable())
            {
                return;
            }

            _clip = clip;
            PlayInternal();
        }

        public void Stop()
        {
            if (NotAvailable())
            {
                return;
            }

            _volumeTransition?.Kill();
            _volumeTransition = null;
            _source1.Player.Stop();
            _source2.Player.Stop();
            _overlapTransition?.Kill();
            _overlapTransition = null;
        }

        public void SetVolume(float value)
        {
            if (NotAvailable())
            {
                return;
            }

            _source1.Player.volume = value;
            _source2.Player.volume = value;
        }

        public void SetMute(bool value)
        {
            if (NotAvailable())
            {
                return;
            }

            _source1.Player.mute = value;
            _source2.Player.mute = value;
        }

        public void VolumeTransition(float volume, float duration = 1.5f, Action onComplete = null)
        {
            if (NotAvailable())
            {
                onComplete?.Invoke();
                return;
            }
            
            if (Math.Abs(Current.Player.volume - volume) < 0.001f)
            {
                onComplete?.Invoke();
                return;
            }

            _volumeTransition?.Kill();
            _volumeTransition = DOTween
                .To(() => Current.Player.volume, SetVolume, volume, duration)
                .OnComplete(() => onComplete?.Invoke())
                .SetAutoKill(true);
        }
        
        private void PlayInternal()
        {
            if (NotAvailable() || !_clip)
            {
                return;
            }

            Current.Player.clip = _clip;
            Current.Player.Play();
            _pingPong = !_pingPong;
            _overlapTransition?.Kill();
            _overlapTransition = DOVirtual
                .DelayedCall(_clip.length - _overlapTime, PlayInternal)
                .SetAutoKill(false)
                .Play();
        }

        private bool NotAvailable()
        {
            if (_source1 && _source2) return false;
            Debug.LogError($"Audio players unavailable");
            return true;
        }
    }
}