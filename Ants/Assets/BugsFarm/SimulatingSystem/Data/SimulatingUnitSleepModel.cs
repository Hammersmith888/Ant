using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct SimulatingUnitSleepModel : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public float Duration;
    }
}