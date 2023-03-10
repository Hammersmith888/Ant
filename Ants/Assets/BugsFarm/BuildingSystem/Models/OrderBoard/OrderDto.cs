using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using BugsFarm.Services.StorageService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class OrderDto : IStorageItem, ISerializationCallbackReceiver
    {
        [field: NonSerialized] public event Action<string> OnStageChanged;
        public OrderStage Stage
        {
            get => _stage;
            set
            {
                if (_stage == value) return;
                _stage = value;
                OnStageChanged?.Invoke(OrderID);
            }
        }
        string IStorageItem.Id => OrderID;
        public string OrderID;
        public OrderItemParam LifeTime;
        public OrderItemParam NextOrderTime;
        public OrderItemParam[] Rewards;
        [NonSerialized] public Dictionary<string, OrderItemDto> Items;
        [SerializeField] private OrderItemDto[] _items;
        [SerializeField] private OrderStage _stage = OrderStage.Process;

    #region Serialization Helper
        [OnSerializing]
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            _items = Items?.Values.ToArray() ?? new OrderItemDto[0];
        }
        [OnDeserialized]
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Items = _items?.ToDictionary(x => x.ItemID) ?? new Dictionary<string, OrderItemDto>();
        }
    #endregion

    }
}