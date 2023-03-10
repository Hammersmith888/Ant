using System;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class LarvaPointDto : IStorageItem
    {
        public string Id => Guid;

        public string Guid;
        public float X;
        public float Y;
    }

    public class LarvaPointDtoStorage : Storage<LarvaPointDto>
    {
        public LarvaPointDtoStorage(ISavableStorage savableStorage)
        {
            savableStorage.Register(this);
        }
    }
}
