using System;
using System.Collections.Generic;
using BugsFarm.ReloadSystem;
using BugsFarm.UnitSystem;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    public abstract class BaseDeathSystem<T>
    {
        protected Dictionary<string, DeathController> _storage;
        
        protected readonly Dictionary<string, string> _deadObjects;
        protected readonly List<string> _dyingObjects;
        
        protected readonly IInstantiator _instantiator;
        private CompositeDisposable _compositeDisposable;

        protected BaseDeathSystem(IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _compositeDisposable = new CompositeDisposable();
            _storage = new Dictionary<string, DeathController>();
            _deadObjects = new Dictionary<string, string>();
            _dyingObjects = new List<string>();

            MessageBroker.Default.Receive<T>().Subscribe(OnDeadly).AddTo(_compositeDisposable);
            MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloads).AddTo(_compositeDisposable);
        }

        private void OnGameReloads(GameReloadingReport gameReloadingReport)
        {
            _compositeDisposable.Dispose();
        }
        public void Register(string guid, params string[] statKeys)
        {
            if (IsRegistered(guid))
            {
                throw new InvalidOperationException($"Registration Unit with [Guid : {guid}], alredy exist.");
            }
            
            _storage.Add(guid, _instantiator.Instantiate<DeathController>(new object[] {guid, statKeys}));
        }

        public abstract void Unregister(string guid);
        
        public bool IsRegistered(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public bool IsDead(string guid)
        {
            return _deadObjects.ContainsKey(guid);
        }

        public bool IsRunned(string guid)
        {
            return _dyingObjects.Contains(guid);
        }

        protected abstract void OnDeadly(T deathProtocol);
    }
}