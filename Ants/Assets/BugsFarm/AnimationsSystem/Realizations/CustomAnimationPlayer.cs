using System;
using System.Collections.Generic;

namespace BugsFarm.AnimationsSystem
{
    public class CustomAnimationPlayer
    {
        public event Action OnComplete;
        public readonly ISpineAnimator Animator;
        private List<CustomAnimationClip> _clip;
        private bool _loop;
        
        private CustomAnimationClip _current;
        private int _playPosition;
        private bool _initialized;
        public CustomAnimationPlayer(AnimatorStorage animatorStorage, string entityId)
        {
            Animator = animatorStorage.Get(entityId);
        }

        public void SetAnimationClips(bool loop, params CustomAnimationClip[] playModels)
        {
            _loop = loop;
            _clip = new List<CustomAnimationClip>(playModels);
        }
        
        public void Play()
        {
            if(_initialized) return;
            Animator.OnAnimationComplete += OnAnimationComplete;
            _initialized = true;
            _playPosition = 0;
            _current = default;
            Next();
        }
        public void Stop()
        {
            if(!_initialized) return;
            
            _initialized = false;
            _current = default;
            _playPosition = 0;
            Animator.OnAnimationComplete -= OnAnimationComplete;
        }

        private void Next()
        {
            while (true)
            {
                if (!_initialized) return;
                
                if (_playPosition >= _clip.Count)
                {
                    if (!_loop)
                    {
                        Stop();
                        OnComplete?.Invoke();
                        return;
                    }
                    _playPosition = 0;
                }
                
                if (!_current.Initialized)
                {
                    _current = _clip[_playPosition];
                }

                if (_current.Iteration <= 0)
                {
                    _playPosition++;
                    _current = default;
                    continue;
                }

                _current.Iteration--;
                Animator.SetAnim(_current.AnimKey);
                break;
            }
        }

        private void OnAnimationComplete(AnimKey anim)
        {
            if(!_initialized) return;
            Next();
        }
    }
}