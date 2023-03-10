using System.Collections.Generic;

namespace BugsFarm.Services.SaveManagerService
{
    public class SavableStorage : ISavableStorage
    {
        private readonly List<ISavable> _savables = new List<ISavable>();

        public void Register(ISavable savable)
        {
            if(_savables.Contains(savable)) return;
            _savables.Add(savable);
        }

        public void Unregister(ISavable savable)
        {
            _savables.Remove(savable);
        }
        public IEnumerable<ISavable> GetAll()
        {
            return _savables.ToArray();
        }

        public T Get<T>() where T : ISavable
        {
            throw new System.NotImplementedException();
        }

        public void Clear()
        {
            _savables.Clear();
        }
    }
}