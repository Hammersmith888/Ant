using UnityEngine;
using Zenject;

namespace BugsFarm.Services.MonoPoolService
{
    public interface IMonoPoolable : IPoolable
    {
        GameObject GameObject { get; }
    }
}