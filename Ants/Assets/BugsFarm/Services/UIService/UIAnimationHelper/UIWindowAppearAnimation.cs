using System;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Services.UIService
{
    public class UIWindowAppearAnimation : UIBaseAnimation
    {
        [SerializeField] protected Vector2 _from;
        [SerializeField] protected Vector2 _to;

        private Tween _t;

        public override void Play(Action onEnd = null)
        {
            Stop();

            _t = _body
                .DOAnchorPos(_to, _settings.durationIn)
                .SetDelay(_settings.delay)
                .SetEase(_settings.easeIn)
                .OnComplete(() => { onEnd?.Invoke(); });

            _t.SetAutoKill(false);
        }

        public override void Backward(Action onEnd = null)
        {
            if (_t == null)
            {
                return;
            }

            Stop();

            _t = _body
                .DOAnchorPos(_from, _settings.durationOut)
                .SetDelay(_settings.delay)
                .SetEase(_settings.easeOut)
                .OnComplete(() => { onEnd?.Invoke(); });
        }

        public override void ResetValues()
        {
            Stop();

            _t?.Kill();

            _body.anchoredPosition = _from;
            _t = null;
        }

        private void Stop()
        {
            _t?.Pause();
        }
    }
}