using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct SimulatingFoodOrderModel : IStorageItem
    {
        public string Id => ModelID;
        public string ModelID;
        public int Priority;
    }
}