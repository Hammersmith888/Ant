using System;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingOpenablesModel : IStorageItem
    {
        string IStorageItem.Id => RoomID;
        public string RoomID;
        public BuildingOpenableModel[] Items;
    }
}