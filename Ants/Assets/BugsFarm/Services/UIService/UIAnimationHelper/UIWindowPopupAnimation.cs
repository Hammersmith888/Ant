using System;
using DG.Tweening;
using UnityEngine;

namespace BugsFarm.Services.UIService
{
    public class UIWindowPopupAnimation : UIBaseAnimation
    {
        [SerializeField] protected Vector2 _from;
        [SerializeField] protected Vector2 _to;
        private Tween _t;

        public override void Play(Action onEnd = null)
        {
            Stop();

            var settings = _settings;

            _t = _body
                .DOScale(_to, settings.durationIn)
                .SetDelay(settings.delay)
                .SetEase(settings.easeIn)
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

            var settings = _settings;

            _t = _body
                .DOScale(_from, settings.durationOut)
                .SetDelay(settings.delay)
                .SetEase(settings.easeOut)
                .OnComplete(() => { onEnd?.Invoke(); });
        }

        public override void ResetValues()
        {
            Stop();
            _t?.Kill();
            _body.localScale = _from;
            _t = null;
        }

        private void Stop()
        {
            _t?.Pause();
        }
    }
}