using System;
using System.Collections.Generic;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitFallSystem : IUnitFallSystem
    {
        public event Action<string> OnComplete;
        private readonly UnitTaskProcessorStorage _taskProcessorStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly IInstantiator _instantiator;

        private readonly List<string> _storage;
        private readonly Dictionary<string, ITask> _fallUnits;
        
        public UnitFallSystem(UnitTaskProcessorStorage taskProcessorStorage,
                              UnitMoverStorage unitMoverStorage,
                              IInstantiator instantiator)
        {
            _taskProcessorStorage = taskProcessorStorage;
            _unitMoverStorage = unitMoverStorage;
            _instantiator = instantiator;
            _storage = new List<string>();
            _fallUnits = new Dictionary<string, ITask> ();
        }

        public void Registration(string unitGuid)
        {
            if (HasEntity(unitGuid))
            {
                throw new ArgumentException($"{this} : Unit with [Guid : {unitGuid}], alredy exist.");
            }

            if (!_unitMoverStorage.HasEntity(unitGuid))
            {
                throw new InvalidOperationException($"{this} : Unit with [Guid : {unitGuid}], does not have {nameof(IUnitMover)}.");
            }
            _storage.Add(unitGuid);
            if (_unitMoverStorage.Get(unitGuid).IsLoseGround())
            {
                OnLoseGround(unitGuid);
            }
        }

        public void UnRegistration(string unitGuid)
        {
            if (!HasEntity(unitGuid))
            {
                return;
            }

            _storage.Remove(unitGuid);

            if (IsFall(unitGuid) && _taskProcessorStorage.HasEntity(unitGuid))
            {
                var taskProcessor = _taskProcessorStorage.Get(unitGuid);
                taskProcessor.Interrupt();
            }
            else
            {
                _fallUnits.Remove(unitGuid);
            }
        }

        public bool HasEntity(string unitGuid)
        {
            return _storage.Contains(unitGuid);
        }

        public bool IsFall(string unitGuid)
        {
            return _fallUnits.ContainsKey(unitGuid);
        }

        public void OnLoseGround(string unitGuid)
        {
            if (!HasEntity(unitGuid))
            {
                return;
            }
            if (IsFall(unitGuid))
            {
                var task = _fallUnits[unitGuid];
                if (task.IsNullOrDefault() || task.IsCompleted)
                {
                    _fallUnits.Remove(unitGuid);
                }
                else
                {
                    return;
                }
            }
            
            
            var taskProcessor = _taskProcessorStorage.Get(unitGuid);
            var fallTask = _instantiator.Instantiate<FallUnitTask>();
            _fallUnits.Add(unitGuid, fallTask);
            fallTask.OnComplete  += _ => OnFallTaskEnd(unitGuid);
            fallTask.OnInterrupt += _ => OnFallTaskEnd(unitGuid);
            taskProcessor.SetTask(fallTask);
        }

        private void OnFallTaskEnd(string unitGuid)
        {
            if (!IsFall(unitGuid)) return;
                
            _fallUnits.Remove(unitGuid);
            OnComplete?.Invoke(unitGuid);
        }
    }
}