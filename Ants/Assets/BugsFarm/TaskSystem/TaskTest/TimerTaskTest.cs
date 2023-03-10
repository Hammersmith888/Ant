﻿using System;
using BugsFarm.SimulationSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class TimerTaskTest : BaseTask, ITickable
    {
        private readonly ITickableManager _tickableManager;
        private readonly ISimulationSystem _simulationSystem;
        private readonly bool _hasSimulate;
        private float _chunkTime;
        private float _chunkTimer;
        private bool _hasChunkAction;
        private Action _onChunkComplete;
        private Action<float> _onUpdate;
        private float _timer;
        private float _reference;

        public TimerTaskTest(ITickableManager tickableManager,
                             ISimulationSystem simulationSystem,
                             bool hasSimulate = true)
        {
            _tickableManager = tickableManager;
            _simulationSystem = simulationSystem;
            _hasSimulate = hasSimulate;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            _timer = Mathf.Abs((float) args[0]);
            if (_timer == 0)
            {
                Completed();
                return;
            }

            base.Execute(args);
            _tickableManager.Add(this);
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

        public void Tick()
        {
            if (!IsRunned || (_simulationSystem.Simulation && !_hasSimulate))
            {
                return;
            }

            var delta = _simulationSystem.DeltaTime;
            _timer -= delta;

            if (_hasChunkAction)
            {
                _chunkTimer -= delta;
                if (_chunkTimer <= 0)
                {
                    // дельта времени может быть больше чанки
                    // по этому нужно расчитать сколько выполненно чанков в этом кадре
                    // а остаток отправить в следующий кадр
                    // позволит корректно выполнить чанки в симуляциях
                    var tempChunkTime = Mathf.Abs(_chunkTimer) + _chunkTime;
                    while (tempChunkTime >= _chunkTime)
                    {
                        _onChunkComplete?.Invoke();
                        tempChunkTime -= _chunkTime;
                    }

                    _chunkTimer = tempChunkTime + _chunkTime;
                }
            }
            _onUpdate?.Invoke(_timer);
            if (_timer <= 0)
            {
                Completed();
            }
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _tickableManager.Remove(this);
            }

            _onChunkComplete = null;
            _onUpdate = null;
        }

    }
}