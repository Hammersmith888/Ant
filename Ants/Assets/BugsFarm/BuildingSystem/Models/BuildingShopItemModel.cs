using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public struct BuildingShopItemModel : IStorageItem, ISerializationCallbackReceiver
    {
        string IStorageItem.Id => ModelId;
        public int TypeId;
        public string ModelId;
        public CurrencyModel Price;
        public Dictionary<int, ShopItemLevelModel> Levels;
        [SerializeField] private ShopItemLevelModel[] _levels;

    #region Serialization Helper
        [OnSerializing]
        public void OnBeforeSerialize()
        {
            _levels = Levels?.Values.ToArray() ?? new ShopItemLevelModel[0];
        }
        
        [OnDeserialized]
        public void OnAfterDeserialize()
        {
            Levels = _levels?.ToDictionary(x => x.Level) ?? new Dictionary<int, ShopItemLevelModel>();
        }
    #endregion
    }
}