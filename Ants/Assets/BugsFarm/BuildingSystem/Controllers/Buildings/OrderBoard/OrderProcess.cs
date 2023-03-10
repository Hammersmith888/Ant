using System.Collections.Generic;
using System.Linq;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class OrderProcess : State
    {
        private readonly IStateMachine _orderStateMachine;
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly OrderModelsStorage _orderModelsStorage;
        private readonly GetResourceSystem _getResourceSystem;
        private readonly StatsCollectionStorage _statCollectionStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;
        private OrderDto _orderDto;
        private ITask _timer;

        public OrderProcess(IStateMachine orderStateMachine,
                            OrderDtoStorage orderDtoStorage,
                            OrderModelsStorage orderModelsStorage,
                            GetResourceSystem getResourceSystem,
                            StatsCollectionStorage statCollectionStorage,
                            BuildingDtoStorage buildingDtoStorage,
                            UnitDtoStorage unitDtoStorage,
                            IInstantiator instantiator) : base(OrderStage.Process.ToString())
        {
            _orderStateMachine = orderStateMachine;
            _orderDtoStorage = orderDtoStorage;
            _orderModelsStorage = orderModelsStorage;
            _getResourceSystem = getResourceSystem;
            _statCollectionStorage = statCollectionStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _unitDtoStorage = unitDtoStorage;
            _instantiator = instantiator;
        }

        public override void OnEnter(params object[] args)
        {
            var orderId = (string) args[0];
            if (_orderDto == null)
            {
                _orderDto = _orderDtoStorage.Get(orderId);
                _orderDto.OnStageChanged += OnOrderStageChanged;
                foreach (var itemDto in _orderDto.Items.Values)
                {
                    itemDto.OnStageChanged += OnItemStageChanged;
                }
            }

            if (_timer == null)
            {
                var timer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
                timer.OnComplete += _ => OnEnter(orderId);
                timer.SetUpdateAction(left => _orderDto.LifeTime.CurrentValue = left);
                _timer = timer;
                _timer.Execute(_orderDto.LifeTime.CurrentValue);
            }

            if (_orderDto.Items.Values.All(x => x.Stage == OrderItemStage.Compelte))
            {
                OnRewards();
                return;
            }

            if (_orderDto.LifeTime.CurrentValue <= 0)
            {
                if (_orderDto.Items.Values.Any(x => x.Stage == OrderItemStage.Compelte))
                {
                    OnRewards();
                    return;
                }

                _orderDto.Stage = OrderStage.WaitNextOrder;
            }
        }

        public override void OnExit()
        {
            if (_orderDto != null)
            {
                _orderDto.OnStageChanged -= OnOrderStageChanged;
                foreach (var itemDto in _orderDto.Items.Values)
                {
                    itemDto.OnStageChanged -= OnItemStageChanged;
                }
            }

            _orderDto = null;
            _timer?.Interrupt();
            _timer = null;
        }

        private bool ValidateStatByParam(OrderItemParam param, string guid)
        {
            var statsCollection = _statCollectionStorage.Get(guid);
            if (!statsCollection.HasEntity("stat_" + param.Key)) return true;
            var statValue = statsCollection.GetValue("stat_" + param.Key);
            return statValue >= param.CurrentValue && statValue <= param.BaseValue;
        }

        private void OnRewards()
        {
            if (_orderDto == null || _orderDto.Stage.AnyOff(OrderStage.Reward, OrderStage.Rewarding))
            {
                return;
            }
            
            var itemModels = _orderModelsStorage.Get(_orderDto.OrderID).Items.ToDictionary(x=> x.ItemId);
            var itemDtos = _orderDto.Items.Values.Where(x => x.Stage == OrderItemStage.Compelte);
            var rewards = new Dictionary<string, OrderItemParam>();
            foreach (var itemDto in itemDtos)
            {
                var itemModel = itemModels[itemDto.ItemID];
                foreach (var rewardModel in itemModel.Rewards)
                {
                    if (!rewards.ContainsKey(rewardModel.Key))
                    {
                        rewards.Add(rewardModel.Key, new OrderItemParam
                        {
                            Id = _orderDto.OrderID,
                            Key = rewardModel.Key,
                        });
                    }

                    var rewardItem = rewards[rewardModel.Key];
                    rewardItem.BaseValue += rewardModel.BaseValue;
                    rewardItem.CurrentValue = rewardItem.BaseValue;
                }
            }

            _orderDto.Rewards = rewards.Values.ToArray();
            _orderDto.Stage = OrderStage.Reward;
        }

        private void OnItemStageChanged(string itemId)
        {
            if (_orderDto == null || !_orderDto.Items.ContainsKey(itemId))
            {
                return;
            }

            var itemDto = _orderDto.Items[itemId];
            if (itemDto.Stage == OrderItemStage.Process)
            {
                var paramCount = itemDto.Params["count"];
                if (itemDto.IsUnit)
                {
                    foreach (var unitDto in _unitDtoStorage.Get().ToArray())
                    {
                        if (unitDto.ModelID != itemDto.ModelID)
                        {
                            continue;
                        }
                        
                        if (itemDto.Params.Values.All(param => ValidateStatByParam(param, unitDto.Guid)))
                        {
                            paramCount.CurrentValue++;
                            _instantiator.Instantiate<DeleteUnitCommand>()
                                .Execute(new DeleteUnitProtocol(unitDto.Guid));
                            if (paramCount.CurrentValue >= paramCount.BaseValue)
                            {
                                itemDto.Stage = OrderItemStage.Compelte;
                                return;
                            }
                        }
                    }
                }
                else
                {
                    const string itemModelId = "0"; // пока все предметы - еда..
                    foreach (var buildingDto in _buildingDtoStorage.Get())
                    {
                        if (buildingDto.ModelID != itemDto.ModelID) continue;
                        var collectCount = (int) (paramCount.BaseValue - paramCount.CurrentValue);
                        _getResourceSystem.GetImmediateItems(itemModelId, ref collectCount, buildingDto.Guid);
                        paramCount.CurrentValue += collectCount;
                        if (paramCount.CurrentValue >= paramCount.BaseValue)
                        {
                            itemDto.Stage = OrderItemStage.Compelte;
                            return;
                        }
                    }
                }
            }

            OnEnter(_orderDto.OrderID);
        }

        private void OnOrderStageChanged(string orderId)
        {
            _orderStateMachine.Switch(_orderDto.Stage.ToString(), orderId);
        }
    }
}