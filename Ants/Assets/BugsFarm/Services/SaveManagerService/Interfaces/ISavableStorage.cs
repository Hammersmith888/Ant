using System.Collections.Generic;

namespace BugsFarm.Services.SaveManagerService
{
    public interface ISavableStorage
    {
        void Register(ISavable savable);
        void Unregister(ISavable savable);
        IEnumerable<ISavable> GetAll();
        T Get<T>() where T : ISavable;
        void Clear();
    }
}