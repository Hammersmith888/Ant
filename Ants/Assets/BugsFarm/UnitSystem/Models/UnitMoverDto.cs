using System;
using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using BugsFarm.Services.StorageService;
using UnityEngine;
using UnityEngine.Serialization;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public class UnitMoverDto : IStorageItem
    {
        public string Guid;
        public string ModelID;
        public Vector3S Position;
        public Vector3S Rotation;
        public Vector3S Normal;
        public LocationLayer Layer;
        string IStorageItem.Id => Guid;
    }
}