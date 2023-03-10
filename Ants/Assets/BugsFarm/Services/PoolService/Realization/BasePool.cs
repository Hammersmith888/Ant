using System;
using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BugsFarm.Services.PoolService
{
    public abstract class BasePool : IPool
    {
        private DiContainer _diContainer;
        private Dictionary<Type, List<IPoolable>> _pool;

        [Inject]
        private void Inject(DiContainer diContainer)
        {
            _diContainer = diContainer;
            _pool = new Dictionary<Type, List<IPoolable>>();
        }

        public object Get(Type concreteType, params object[] args)
        {
            if (concreteType == null)
            {
                throw new ArgumentException("ConcreteType is missing");
            }

            if (concreteType.GetInterfaces().All(x => x != typeof(IPoolable)))
            {
                throw new ArgumentException("ConcreteType must implement IPoolable interface");
            }
            
            if (!_pool.ContainsKey(concreteType))
            {
                _pool.Add(concreteType, new List<IPoolable>());
            }

            var poolables = _pool[concreteType];
            if (poolables.Count == 0)
            {
                return Create(concreteType, args);
            }

            var poolable = poolables[0];
            poolables.RemoveAt(0);
            _diContainer.Inject(args);
            return poolable;
        }

        public T Get<T>(params object[] args ) where T : IPoolable
        {
            return (T)Get(typeof(T), args);
        }

        public void Return(IPoolable polable)
        {
            if (polable == null)
            {
                return;
            }
            
            var type = polable.GetType();
            if (!_pool.ContainsKey(type))
            {
                _pool.Add(type, new List<IPoolable>());
            }

            if (_pool[type].Contains(polable))
            {
                return;
            }
            _pool[type].Add(polable);
            polable.Relese();
        }
        
        public object Create(Type concreteType, params object[] args)
        {
            return _diContainer.Instantiate(concreteType, args);
        }
    }
}