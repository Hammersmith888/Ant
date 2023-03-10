using BugsFarm.Services.StorageService;

namespace BugsFarm.Services.InteractorSystem
{
    public interface IInteractorService : IStorageItem
    {
        void Init();
        void Dispose();
    }
}