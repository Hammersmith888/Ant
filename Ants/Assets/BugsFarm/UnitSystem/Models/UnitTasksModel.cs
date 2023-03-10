using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitTasksModel : IStorageItem
    {
        public string ModelID;
        /// <summary>
        /// Определяет исполняемые поведения, а последовательность определяет приоритет :
        /// чем меньше индекс задачи, тем больше приоритет исполнения.
        /// </summary>
        public string[] Tasks;

        public string[] AssignTasks;
        
        public bool IsAssignable;
        string IStorageItem.Id => ModelID;
    }
}