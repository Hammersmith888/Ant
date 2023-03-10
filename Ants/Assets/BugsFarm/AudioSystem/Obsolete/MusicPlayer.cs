using DG.Tweening;
using UnityEngine;

namespace BugsFarm.AudioSystem.Obsolete
{
    public class MusicPlayer : MonoBehaviour
    {
        [SerializeField] private bool _stopAtStart;
        [SerializeField] private AudioClip _music;
        [SerializeField] private float _fadeDuration;
        [SerializeField] private float _overlapTime;

        [SerializeField] private AudioSource _audioSource_1;
        [SerializeField] private AudioSource _audioSource_2;

        public bool Mute => _mute;
        
        private double _nextStartTime;
        private bool _pingPong;

        private float _volumeMax;
        private float _volumeCur;

        private Tween _tween;
        private bool _stopped;
        private bool _mute;

        public void Play()
        {
            Stop();

            _stopped = false;
            _nextStartTime = 0;

            SetVolume(_volumeMax);
            SetMute(false);

            Update();
        }
        public void Stop()
        {
            _stopped = true;

            _audioSource_1.Stop();
            _audioSource_2.Stop();
        }
        public void FadeMute(bool mute)
        {
            if (!mute)
                SetMute(false);
            _tween?.Kill();
            _tween = DOTween.To( () => _volumeCur, SetVolume, mute ? 0 : _volumeMax, _fadeDuration).OnComplete(() => SetMute(mute));
        }
        
        private void Start()
        {
            _volumeMax = _volumeCur = _audioSource_1.volume;
            _stopped = _stopAtStart;
        }
        private void Update()
        {
            if ( _stopped || AudioSettings.dspTime < _nextStartTime )
                return;

            var audioSource = _pingPong ? _audioSource_1 : _audioSource_2;

            audioSource.clip = _music;
            audioSource.Play();

            _nextStartTime = AudioSettings.dspTime + _music.length - _overlapTime;
            _pingPong ^= true;
        }
        private void SetVolume(float volume)
        {
            _volumeCur = volume;
            _audioSource_1.volume = volume;
            _audioSource_2.volume = volume;
        }
        public void SetMute(bool mute)
        {
            _audioSource_1.mute = mute;
            _audioSource_2.mute = mute;
            _mute = mute;
        }
    }
}

