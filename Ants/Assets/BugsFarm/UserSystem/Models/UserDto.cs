using System;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.UserSystem
{
    [Serializable]
    public class UserDto : IStorageItem
    {
        public string Id => _id;

        public bool AcceptedPPA;

        public bool Tutorial;
        
        [SerializeField] private string _id;

        public UserDto(string id)
        {
            _id = id;
            Tutorial = true;
            AcceptedPPA = false;
        }
    }
}