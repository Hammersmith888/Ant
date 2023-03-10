using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UserSystem;

namespace BugsFarm.SimulatingSystem
{
    public class UpdateOrdersSimulatingStage
    {
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly OrderUsedStorage _orderUsedStorage;
        private readonly IUser _user;
        private readonly OrderModelsStorage _orderModelsStorage;
        private double _simulatingTimeInSeconds;
        private double _pastGameAge;

        private const float _dayInSeconds = 86400.0f;
        private const float _sixHoursInMinutes = 360.0f;
        
        public UpdateOrdersSimulatingStage(OrderDtoStorage orderDtoStorage, 
                                           OrderUsedStorage orderUsedStorage,
                                           OrderModelsStorage orderModelsStorage,
                                           IUser user)
        {
            _orderUsedStorage = orderUsedStorage;
            _orderModelsStorage = orderModelsStorage;
            _user = user;
            _orderDtoStorage = orderDtoStorage;
        }

        public void UpdateOrders(double simulatingTimeInSeconds, double pastGameAge)
        {
            _simulatingTimeInSeconds = simulatingTimeInSeconds;
            _pastGameAge = pastGameAge;
            
            foreach (var orderDto in _orderDtoStorage.Get().ToArray())
            {
                UpdateOrder(orderDto);
            }
        }

        private double GetSimulatingTimeInMinutes() => _simulatingTimeInSeconds / 60.0;
        private int GetPastDay() => (int) (_pastGameAge / _dayInSeconds) + 1;
        private int GetCurrentDay() =>  (int)((_pastGameAge + _simulatingTimeInSeconds) / _dayInSeconds) + 1;
        
        private void UpdateOrder(OrderDto orderDto)
        {
            if (orderDto.Stage == OrderStage.Process)
            {
                UpdateProcessingOrder(orderDto);
            }
            else if (orderDto.Stage == OrderStage.WaitNextOrder)
            {
                UpdateWaitingOrder(orderDto);
            }
        }

        private void UpdateWaitingOrder(OrderDto orderDto)
        {
            double simulatingTimeInMinutes = GetSimulatingTimeInMinutes();
            if  (orderDto.NextOrderTime.CurrentValue <= simulatingTimeInMinutes)
            {
                orderDto.NextOrderTime.CurrentValue = 0;
                orderDto.Stage = OrderStage.Finalized;
                _orderDtoStorage.Remove(orderDto);
                var newOrder = CreateNewOrder(_orderModelsStorage.Get(orderDto.OrderID).IsSpecial);
                var remainder = (_pastGameAge + _simulatingTimeInSeconds) - _dayInSeconds * (GetCurrentDay()-1);
                newOrder.NextOrderTime.CurrentValue -= (float)remainder/ 60.0f;
            }
            else
            {
                orderDto.NextOrderTime.CurrentValue -= (float)simulatingTimeInMinutes;

            }
        }

        private void UpdateProcessingOrder(OrderDto orderDto)
        {
            double simulatingTimeInMinutes = GetSimulatingTimeInMinutes();
            if  (orderDto.LifeTime.CurrentValue <= simulatingTimeInMinutes)
            {
                orderDto.LifeTime.CurrentValue = 0;
                orderDto.Stage = OrderStage.Finalized;
                _orderDtoStorage.Remove(orderDto);
                var newOrder = CreateNewOrder(_orderModelsStorage.Get(orderDto.OrderID).IsSpecial);
                
                
                var remainder = (_pastGameAge + _simulatingTimeInSeconds) - _dayInSeconds * (GetCurrentDay()-1);
                newOrder.LifeTime.CurrentValue -= (float)remainder/ 60.0f;
            }
            else
            {
                orderDto.LifeTime.CurrentValue -= (float)simulatingTimeInMinutes;
            }
        }

        private OrderDto CreateNewOrder(bool isSpecial)
        {
            var orderIds = GetAvailableOrders(isSpecial).ToArray();
            var nextOrderTime = _sixHoursInMinutes;

            var orderId = Tools.RandomItem(orderIds);
            var orderModel = _orderModelsStorage.Get(orderId);
            OrderDto orderDto = new OrderDto();
            orderDto.OrderID = orderId;
            orderDto.Rewards = new OrderItemParam[0];
            
            orderDto.LifeTime = new OrderItemParam
            {
                Id = orderId,
                CurrentValue = orderModel.LifeTime,
                BaseValue = orderModel.LifeTime,
            };
            
            orderDto.NextOrderTime = new OrderItemParam
            {
                Id = orderId,
                CurrentValue = nextOrderTime,
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
           // _orderUsedStorage.Add(new OrderUsed(orderId));
            orderDto.Stage = OrderStage.Process;
            return orderDto;
            
        }
        private IEnumerable<string> GetAvailableOrders(bool isSpecial)
        {
            var maxLevel = _user.GetLevel();
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

                yield return orderModel.OrderID;
            }
        }
    }
}