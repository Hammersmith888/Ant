using BugsFarm.Services.StateMachine;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class OrderWaitNextOrder : State
    {
        private readonly IStateMachine _orderStateMachine;
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly OrderModelsStorage _orderModelsStorage;
        private readonly IInstantiator _instantiator;
        private OrderDto _orderDto;
        private ITask _timer;

        public OrderWaitNextOrder(IStateMachine orderStateMachine,
                                  OrderDtoStorage orderDtoStorage,
                                  OrderModelsStorage orderModelsStorage,
                                  IInstantiator instantiator) : base(OrderStage.WaitNextOrder.ToString())
        {
            _orderStateMachine = orderStateMachine;
            _orderDtoStorage = orderDtoStorage;
            _orderModelsStorage = orderModelsStorage;
            _instantiator = instantiator;
        }

        public override void OnEnter(params object[] args)
        {
            var orderId = (string) args[0];
            if (_orderDto == null)
            {
                _orderDto = _orderDtoStorage.Get(orderId);
                _orderDto.OnStageChanged += OnStageChanged;
            }

            if (_timer == null)
            {
                var timer = _instantiator.Instantiate<SimulatedTimerTask>(new object[]{TimeType.Minutes});
                timer.OnComplete += _ => OnEnter(orderId);
                timer.SetUpdateAction(left => _orderDto.NextOrderTime.CurrentValue = left);
                _timer = timer;
                _timer.Execute(_orderDto.NextOrderTime.CurrentValue);
            }

            if (_orderDto.NextOrderTime.CurrentValue <= 0)
            {
                _orderDto.Stage = OrderStage.Finalized;
            }
        }

        public override void OnExit()
        {
            if (_orderDto != null)
            {
                _orderDto.OnStageChanged -= OnStageChanged;
            }

            _orderDto = null;
            _timer?.Interrupt();
            _timer = null;
        }

        private void OnStageChanged(string orderId)
        {
            _orderStateMachine.Switch(_orderDto.Stage.ToString(), orderId);
        }
    }
}