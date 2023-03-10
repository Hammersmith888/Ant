using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BattleSystem
{
    [Serializable]
    public struct BattlePassModel : ISerializationCallbackReceiver, IStorageItem
    {
        string IStorageItem.Id => ModelID;
        public string ModelID;
        [NonSerialized] public Dictionary<string, BattlePassParam> Params;
        [SerializeField] private BattlePassParam[] _params;

    #region Serialization Helper
        public void OnBeforeSerialize()
        {
            _params = Params?.Values.ToArray();
        }

        public void OnAfterDeserialize()
        {
            Params = _params?.ToDictionary(x => x.Id) ?? new Dictionary<string, BattlePassParam>();
        }
    #endregion
    }
}