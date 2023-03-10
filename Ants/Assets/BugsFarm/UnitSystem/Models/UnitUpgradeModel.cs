using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.Services.StorageService;
using BugsFarm.UpgradeSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    [Serializable]
    public struct UnitUpgradeModel : IStorageItem, ISerializationCallbackReceiver
    {
        string IStorageItem.Id => ModelID;
        public string ModelID;
        [NonSerialized] public Dictionary<int, UpgradeLevelModel> Levels;
        [SerializeField] private UpgradeLevelModel[] _levels;
    
    #region Serialization Helper
        [OnSerializing]
        public void OnBeforeSerialize()
        {
            _levels = Levels?.Values.ToArray() ?? new UpgradeLevelModel[0];
        }

        [OnDeserialized]
        public void OnAfterDeserialize()
        {
            Levels = _levels?.ToDictionary(x => x.Level) ?? new Dictionary<int, UpgradeLevelModel>();
        }
    #endregion
    }
}