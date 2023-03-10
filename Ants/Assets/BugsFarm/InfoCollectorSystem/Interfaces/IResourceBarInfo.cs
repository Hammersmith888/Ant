using BugsFarm.Services.StorageService;

namespace BugsFarm.InfoCollectorSystem
{
    public interface IResourceBarInfo : IStorageItem
    {
        float Progress { get; }
        string CurrencyId { get; }
        /// <summary>
        /// Обновить информацию пример : тик времени, новые состояния.
        /// </summary>
        void Update();
    }
}