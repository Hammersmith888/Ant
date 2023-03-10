using System;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class HospitalSlotDto : IStorageItem
    {
        public string Id => _id;
        [SerializeField] private string _id;
        public string ModelId;
        public HospitalSlotParam LifeTime;
        public HospitalSlotParam RepairTime;
        public bool Repairing;


        public HospitalSlotDto(string id)
        {
            _id = id;
        }
    }
}