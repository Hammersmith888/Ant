using System;
using System.Collections.Generic;
using Zenject;

namespace BugsFarm.SimulationSystem
{
    public class SimulationTickableManger : ITickableManagerInternal, ITickable, IDisposable
    {
        private readonly List<ITickable> _tickables;
        private readonly List<ITickable> _scheduledToRemove;
        private int _thisFrameCountRegistred; // в текущем кадре загеристрированно N кол-во,
                                              // по этому в этом кадре обработаем только их,
                                              // чтобы избежать смертельных циклов -
                                              // сделаем добавление на следующий кадр.

        public SimulationTickableManger()
        {
            _tickables = new List<ITickable>();
            _scheduledToRemove = new List<ITickable>();
        }
        
        public void Add(ITickable tickable)
        {
        #if UNITY_EDITOR
            if (_tickables.Contains(tickable))
            {
                throw new InvalidOperationException("Tickable add twice");
            }
        #endif
            _tickables.Add(tickable);
        }

        public void Remove(ITickable tickable)
        {
        #if UNITY_EDITOR
            if (_scheduledToRemove.Contains(tickable))
            {
                throw new InvalidOperationException("Tickable remove twice");
            }
        #endif
            _scheduledToRemove.Add(tickable);
        }
        
        public void Tick()
        {
            QueueProcess();
            _thisFrameCountRegistred = _tickables.Count;
            UpdateAll();
        }
        void IDisposable.Dispose()
        {
            _tickables.Clear();
            _scheduledToRemove.Clear();
        }
        
        private void UpdateAll()
        {
            for (var i = 0; i < _thisFrameCountRegistred; i++)
            {
                if (!_scheduledToRemove.Contains(_tickables[i]))
                {
                    _tickables[i].Tick();
                }
            }
        }

        private void QueueProcess()
        {
            for (var i = 0; i < _scheduledToRemove.Count; i++)
            {
                _tickables.Remove(_scheduledToRemove[i]);
            }
            _scheduledToRemove.Clear();
        }
    }
}