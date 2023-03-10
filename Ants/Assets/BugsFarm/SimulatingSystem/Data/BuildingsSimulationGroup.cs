using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct BuildingsSimulationGroup : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public string SimulationGroup;
    }
}