using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class OrderItemDto : ISerializationCallbackReceiver
    {
        [field: NonSerialized] public event Action<string> OnStageChanged;
        public OrderItemStage Stage
        {
            get => _stage;
            set
            {
                if (_stage == value) return;
                _stage = value;
                OnStageChanged?.Invoke(ItemID);
            }
        }

        public bool IsUnit;
        public string ItemID;
        public string ModelID;
        public string LocalizationID;
        
        [NonSerialized] public Dictionary<string, OrderItemParam> Params;
        [SerializeField] private OrderItemParam[] _params;
        [SerializeField] private OrderItemStage _stage = OrderItemStage.Idle;
    #region Serialization Helper
        [OnSerializing]
        public void OnBeforeSerialize()
        {
            _params = Params?.Values.ToArray() ?? new OrderItemParam[0];
        }
        [OnDeserialized]
        public void OnAfterDeserialize()
        {
            Params = _params?.ToDictionary(x => x.Key) ?? new Dictionary<string, OrderItemParam>();
        }
    #endregion
    }
}