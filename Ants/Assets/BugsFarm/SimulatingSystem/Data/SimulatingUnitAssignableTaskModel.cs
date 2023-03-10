using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct SimulatingUnitAssignableTaskModel : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public SimulatingAssignableTask[] AssignableTasks;
    }
}