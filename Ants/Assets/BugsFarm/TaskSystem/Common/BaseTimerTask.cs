using System;
using BugsFarm.InfoCollectorSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public abstract class BaseTimerTask : BaseTask, ITickable
    {
        private Action _onChunkComplete;
        private Action<float> _onUpdate;
        private TimeType _timeType;
        private bool _hasChunkAction; // avoid compare Action with null in Tick method.
        private float _chunkTime;
        private float _chunkTimer;
        protected float Timer;

        [Inject]
        private void Inject(TimeType timeType = TimeType.Seconds)
        {
            _timeType = timeType;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            Timer = Mathf.Abs((float) args[0]);
        }

        public void SetChunkAction(Action onChunkComplete, float chunkSeconds)
        {
            _hasChunkAction = true;
            _chunkTime = chunkSeconds;
            _chunkTimer = _chunkTime;
            _onChunkComplete = onChunkComplete;
        }
        
        public void SetUpdateAction(Action<float> onUpdate)
        {
            _onUpdate = onUpdate;
        }
        
        public abstract void Tick();
        
        protected void ApplyTime(float deltaTime)
        {
            if (!IsRunned)
            {
                return;
            }
            
            var modifiedDelta = deltaTime;
            switch (_timeType)
            {
                case TimeType.Minutes:
                    modifiedDelta = Format.SecondsToMinutes(deltaTime);
                    break;
                case TimeType.Hours:
                    modifiedDelta = Format.HoursToSeconds(deltaTime);
                    break;
            }

            if (_hasChunkAction)
            {
                _chunkTimer -= deltaTime;
                if (_chunkTimer <= 0)
                {
                    // дельта времени может быть больше чанки
                    // по этому нужно расчитать сколько выполненно чанков в этом кадре
                    // а остаток отправить в следующий кадр
                    // позволит корректно выполнить чанки в симуляциях
                    var modTimer = Mathf.Abs(_chunkTimer);
                    modTimer += modTimer >= _chunkTime ? 0 : _chunkTime;
                    while (modTimer >= _chunkTime)
                    {
                        modTimer -= _chunkTime;
                        _onChunkComplete.Invoke();
                    }

                    _chunkTimer = modTimer + _chunkTime;
                }
            }
            
            Timer = Mathf.Max(0, Timer - modifiedDelta);
            OnUpdate();
            if (Timer <= 0)
            {
                Completed();
            }
        }

        protected virtual void OnUpdate()
        {
            _onUpdate?.Invoke(Timer);
        }

        protected override void OnForceCompleted()
        {
            Timer = 0;
            OnUpdate();
        }

        protected override void OnDisposed()
        {
            _onChunkComplete = null;
            _onUpdate = null;
        }
    }
}