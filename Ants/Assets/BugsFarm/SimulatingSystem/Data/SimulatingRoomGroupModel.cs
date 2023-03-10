using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.SimulatingSystem
{
    [Serializable]
    public struct SimulatingRoomGroupModel : IStorageItem
    {
        public string Id => TypeName;
        public string TypeName;
        public string Group;
    }
}