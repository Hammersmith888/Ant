using System;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class OrderUsed : IStorageItem
    {
        public string Id => _id;
        [SerializeField] private string _id;
        public OrderUsed(string orderID)
        {
            _id = orderID;
        }
    }
}