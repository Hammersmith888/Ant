using BugsFarm.Services.StorageService;

namespace BugsFarm.InfoCollectorSystem
{
    public interface IResourceInfo : IStorageItem
    {
        string Info { get; }
        /// <summary>
        /// Обновить информацию пример : тик времени, новые состояния.
        /// </summary>
        void Update();
    }
}