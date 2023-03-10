using Zenject;

namespace BugsFarm.SimulationSystem
{
    public interface ITickableManager
    {
        void Add(ITickable tickable);
        void Remove(ITickable tickable);
    }
}