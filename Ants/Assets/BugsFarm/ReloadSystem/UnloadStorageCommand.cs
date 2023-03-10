using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.StorageService;

namespace BugsFarm.ReloadSystem
{
    public class UnloadStorageCommand<T> : Command where T : IStorageItem
    {
        private readonly IStorage<T> _storage;

        public UnloadStorageCommand(IStorage<T> storage)
        {
            _storage = storage;
        }

        public override void Do()
        {
            _storage.Clear();
            OnDone();
        }
    }
}