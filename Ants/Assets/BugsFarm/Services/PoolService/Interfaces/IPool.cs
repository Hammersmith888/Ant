using System;

namespace BugsFarm.Services.PoolService
{
    public interface IPool
    {
        object Get(Type concreteType, params object[] args);
        T Get<T>(params object[] args ) where T : IPoolable;
        void Return(IPoolable polable);
    }
}