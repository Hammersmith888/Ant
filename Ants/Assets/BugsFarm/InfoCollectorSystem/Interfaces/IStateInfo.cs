using System.Collections.Generic;
using BugsFarm.Services.StorageService;

namespace BugsFarm.InfoCollectorSystem
{
    public interface IStateInfo : IStorageItem
    {
        IEnumerable<string> Info { get; }
        /// <summary>
        /// Обновить информацию пример : тик времени, новые состояния.
        /// </summary>
        void Update();
    }
}