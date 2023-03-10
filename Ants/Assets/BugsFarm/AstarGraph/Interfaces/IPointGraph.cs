using System;

namespace BugsFarm.AstarGraph
{
    public interface IPointGraph
    {
        event Action OnUpdate;
        void Initialize();
        void OpenGroupe(string id);
        void Reset();
    }
}