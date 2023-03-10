using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct UnitsSimulationGroupModel : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public string SimulationGroup;
    }
}