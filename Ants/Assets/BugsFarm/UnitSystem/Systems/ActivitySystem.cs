using System;
using System.Collections.Generic;
using BugsFarm.Services.StatsService;

namespace BugsFarm.UnitSystem
{
    public class ActivitySystem : IActivitySystem
    {
        private class Entity
        {
            public event Action<string> OnStateChanged;
            private readonly Action _stopProcess;
            private readonly Action _playProcess;
            private readonly string _guid;
            private readonly StatsCollection _statsCollection;
            private const string _activityStatKey = "stat_activity";
            public Entity(string guid, 
                          StatsCollection statsCollection,
                          Action stopProcess, 
                          Action playProcess)
            {
                
                _guid = guid;
                _stopProcess = stopProcess;
                _playProcess = playProcess;
                _statsCollection = statsCollection;
            }

            public bool Active()
            {
                return _statsCollection.GetValue(_activityStatKey) >= 1;
            }

            public void Activate(bool avitve, bool force)
            {
                if(Active() == avitve && !force) return;
                
                _statsCollection.RemoveAllModifiers(_activityStatKey);
                _statsCollection.AddModifier(_activityStatKey, new StatModBaseAdd(avitve ? 1 : 0));
                
                if (avitve)
                {
                    _playProcess?.Invoke();
                }
                else
                {
                    _stopProcess?.Invoke();
                }
                OnStateChanged?.Invoke(_guid);
            }
        }
        public event Action<string> OnStateChanged;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly Dictionary<string, Entity> _storage;
        
        public ActivitySystem(StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _storage = new Dictionary<string, Entity>();
        }

        public void Registration(ActivitySystemProtocol protocol)
        {
            if (HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(Registration)} :: Alredy exist");
            }

            if (protocol.PlayProcess == null || protocol.StopProcess == null)
            {
                throw new
                    ArgumentException($"{this} : {nameof(Registration)} :: Activity state callbacks does not exist.");
            }

            var statsCollection = _statsCollectionStorage.Get(protocol.Guid);
            var entity = new Entity(protocol.Guid, statsCollection, protocol.StopProcess, protocol.PlayProcess);
            entity.OnStateChanged += OnStateChanged;
            _storage.Add(protocol.Guid, entity);
        }

        public void UnRegistration(string guid)
        {
            if (!HasEntity(guid)) return;
            var entity = _storage[guid];
            entity.OnStateChanged -= OnStateChanged;
            _storage.Remove(guid);
        }

        public void Activate(string guid, bool activate, bool force = false)
        {
            if (!HasEntity(guid)) return;
            _storage[guid].Activate(activate, force);
        }

        public bool IsActive(string guid)
        {
            return HasEntity(guid) && _storage[guid].Active();
        }

        public bool HasEntity(string guid)
        {
            return _storage.ContainsKey(guid);
        }
    }
}