using System;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [Serializable]
    public struct CinematicsState : IStorageItem
    {
        public string Id => _id;
        [SerializeField] private string _id;

        public CinematicsState(string id)
        {
            _id = id;
        }
    }
}