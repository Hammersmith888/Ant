using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingInfoParamsModel : IStorageItem, ISerializationCallbackReceiver
    {
        string IStorageItem.Id => ModelId;
        public string ModelId;
        public Dictionary<string, BuildingInfoParamModel> Items;
        [SerializeField] private BuildingInfoParamModel[] _items;

    #region Serialization Helper

        [OnSerializing]
        public void OnBeforeSerialize()
        {
            _items = Items?.Values.ToArray() ?? new BuildingInfoParamModel[0];
        }
        [OnDeserializing]
        public void OnAfterDeserialize()
        {
            Items = _items?.ToDictionary(x => x.StatId) ?? new Dictionary<string, BuildingInfoParamModel>();
        }
    #endregion
    }
}