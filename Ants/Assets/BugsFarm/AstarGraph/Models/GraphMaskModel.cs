using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [Serializable]
    public struct GraphMaskModel : IStorageItem, ISerializationCallbackReceiver
    {
        string IStorageItem.Id => ModelID;
        public string ModelID;
        public int BitMaskTags;
        [NonSerialized] public Dictionary<uint, MaskModel> Masks;
        [SerializeField] private MaskModel[] _masks;

    #region Serialization Helper
        [OnSerializing]
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _masks = Masks?.Values.ToArray() ?? new MaskModel[0];
        }
        [OnDeserialized]
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Masks = _masks?.ToDictionary(x => x.Tag) ?? new Dictionary<uint, MaskModel>();
        }
    #endregion
    }
}