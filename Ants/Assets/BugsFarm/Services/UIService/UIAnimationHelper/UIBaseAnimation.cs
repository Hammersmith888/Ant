using System;
using UnityEngine;

namespace BugsFarm.Services.UIService
{
    public abstract class UIBaseAnimation : MonoBehaviour
    {
        [SerializeField] private AnimationHelperPreset _preset;
        [SerializeField] protected DOSettings _settings;
        [SerializeField] protected RectTransform _body;
        
        // todo: temp solution for check  animation 
        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.A))
            {
                Play();
            }
            if (Input.GetKeyDown(KeyCode.S))
            {
                Backward();
            }
            
        }
        
        public abstract void Play(Action onEnd = null);

        public abstract void Backward(Action onEnd = null);

        public abstract void ResetValues();

        [ContextMenu("Reset Settings")]
        protected void ResetSetting()
        {
            _settings = new DOSettings();
        }

        private void Awake()
        {
            if (_preset)
            {
                if (_settings.delay == 0)
                {
                    _settings.delay = _preset.Settings.delay;
                }
                if (_settings.durationIn == 0)
                {
                    _settings.durationIn = _preset.Settings.durationIn;
                }
                if (_settings.durationOut == 0)
                {
                    _settings.durationOut = _preset.Settings.durationOut;
                }
                if (_settings.easeIn == 0 )
                {
                    _settings.easeIn = _preset.Settings.easeIn;
                }
                if (_settings.easeOut == 0 )
                {
                    _settings.easeOut = _preset.Settings.easeOut;
                }
            }
        }
    }
}