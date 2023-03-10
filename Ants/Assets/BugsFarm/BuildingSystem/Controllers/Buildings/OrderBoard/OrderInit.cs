using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.UserSystem;

namespace BugsFarm.BuildingSystem
{
    public class OrderInit : State
    {
        private readonly string _buildingId;
        private readonly IUser _currentUser;
        private readonly IStateMachine _orderStateMachine;
        private readonly StatsCollectionStorage _statCollectionStorage;
        private readonly OrderModelsStorage _orderModelsStorage;
        private readonly OrderUsedStorage _orderUsedStorage;
        private readonly OrderDtoStorage _orderDtoStorage;
        
        private const string _nextOrderTimeStatKey = "stat_nextOrderTime";
        private const string _resetCycleLevelStatKey = "stat_resetCycleLevelStatKey";

        public OrderInit(string buildingId,
                         IUser currentUser,
                         IStateMachine orderStateMachine,
                         StatsCollectionStorage statCollectionStorage,
                         OrderModelsStorage orderModelsStorage,
                         OrderUsedStorage orderUsedStorage,
                         OrderDtoStorage orderDtoStorage) : base("Init")
        {
            _buildingId = buildingId;
            _currentUser = currentUser;
            _orderStateMachine = orderStateMachine;
            _statCollectionStorage = statCollectionStorage;
            _orderModelsStorage = orderModelsStorage;
            _orderUsedStorage = orderUsedStorage;
            _orderDtoStorage = orderDtoStorage;
        }

        public override void OnEnter(params object[] args)
        {
            var isSpecial = (bool) args[0];
            var orderDto = _orderDtoStorage.Get().FirstOrDefault(x => _orderModelsStorage.Get(x.OrderID).IsSpecial == isSpecial);
            orderDto = CreateOrder(isSpecial, orderDto);
            _orderStateMachine.Switch(orderDto.Stage.ToString(), orderDto.OrderID);
        }

        private OrderDto CreateOrder(bool isSpecial, OrderDto orderDto = null)
        {
            if (orderDto != null)
            {
                if (orderDto.Stage != OrderStage.Finalized)
                {
                    return orderDto;
                }

                if (_orderDtoStorage.HasEntity(orderDto.OrderID))
                {
                    foreach (var itemDto in orderDto.Items.Values)
                    {
                        foreach (var itemParam in itemDto.Params.Values)
                        {
                            itemParam.Dispose();
                        }
                        itemDto.Params.Clear();
                    }

                    if (orderDto.Rewards != null)
                    {
                        foreach (var itemParam in orderDto.Rewards)
                        {
                            itemParam.Dispose();
                        }
                    }

                    orderDto.Rewards = null;
                    orderDto.Items.Clear();
                    orderDto.LifeTime.Dispose();
                    orderDto.NextOrderTime.Dispose();
                    _orderDtoStorage.Remove(orderDto);
                }
            }
            
            var orderIds = GetAvailableOrders(isSpecial).ToArray();
            if (orderIds.Length > 0)
            {
                var statCollection = _statCollectionStorage.Get(_buildingId);
                var nextOrderTime = statCollection.GetValue(_nextOrderTimeStatKey);

                var orderId = Tools.RandomItem(orderIds);
                var orderModel = _orderModelsStorage.Get(orderId);
                bool isNew = orderDto == null;
                orderDto = orderDto ?? new OrderDto();
                orderDto.OrderID = orderId;
                orderDto.Rewards = new OrderItemParam[0];
                
                orderDto.LifeTime = new OrderItemParam
                {
                    Id = orderId,
                    CurrentValue = !isNew ? UnityEngine.Random.Range(0, orderModel.LifeTime) : orderModel.LifeTime,
                    BaseValue = orderModel.LifeTime,
                };
                
                orderDto.NextOrderTime = new OrderItemParam
                {
                    Id = orderId,
                    CurrentValue = !isNew ? UnityEngine.Random.Range(0, nextOrderTime) : nextOrderTime,
                    BaseValue = nextOrderTime
                };
                orderDto.Items = orderModel.Items.Select(itemModel =>
                {
                    var itemDto = new OrderItemDto
                    {
                        IsUnit = itemModel.IsUnit,
                        ModelID = itemModel.ModelID,
                        ItemID = itemModel.ItemId,
                        Stage = OrderItemStage.Idle,
                        LocalizationID = itemModel.LocalizationID,
                        Params = itemModel.Params.Select(itemParam => new OrderItemParam
                        {
                            Id = itemParam.Id,
                            Key = itemParam.Key,
                            BaseValue = itemParam.BaseValue,
                            CurrentValue = itemParam.CurrentValue,
                        }).ToDictionary(x => x.Key)
                    };
                    
                    return itemDto;
                }).ToDictionary(x=>x.ItemID);
                
                _orderDtoStorage.Add(orderDto);
                _orderUsedStorage.Add(new OrderUsed(orderId));
                orderDto.Stage = OrderStage.Process;
                return orderDto;
            }

            ResetCycleOrders(isSpecial);
            return CreateOrder(isSpecial,orderDto);
        }

        private IEnumerable<string> GetAvailableOrders(bool isSpecial)
        {
            var maxLevel = _currentUser.GetLevel();
            foreach (var orderModel in _orderModelsStorage.Get())
            {
                if (orderModel.IsSpecial != isSpecial)
                {
                    continue;
                }

                if (orderModel.Level > maxLevel)
                {
                    continue;
                }

                if (_orderUsedStorage.HasEntity(orderModel.OrderID))
                {
                    continue;
                }

                yield return orderModel.OrderID;
            }
        }

        private void ResetCycleOrders(bool isSpecial)
        {
            var statCollection = _statCollectionStorage.Get(_buildingId);
            var resetLevel = _currentUser.GetLevel() - statCollection.GetValue(_resetCycleLevelStatKey);
            foreach (var orderUsed in _orderUsedStorage.Get().ToArray())
            {
                var orderModel = _orderModelsStorage.Get(orderUsed.Id);
                if (orderModel.IsSpecial == isSpecial && orderModel.Level >= resetLevel)
                {
                    _orderUsedStorage.Remove(orderUsed);
                }
            }
        }
    }
}