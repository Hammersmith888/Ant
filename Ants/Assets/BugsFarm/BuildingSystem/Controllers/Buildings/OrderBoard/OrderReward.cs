using BugsFarm.Services.StateMachine;

namespace BugsFarm.BuildingSystem
{
    public class OrderReward : State
    {
        private readonly IStateMachine _orderStateMachine;
        private readonly OrderDtoStorage _orderDtoStorage;
        private OrderDto _orderDto;

        public OrderReward(IStateMachine orderStateMachine,
                           OrderDtoStorage orderDtoStorage) : base(OrderStage.Reward.ToString())
        {
            _orderStateMachine = orderStateMachine;
            _orderDtoStorage = orderDtoStorage;
        }

        public override void OnEnter(params object[] args)
        {
            var orderId = (string) args[0];
            if (_orderDto == null)
            {
                _orderDto = _orderDtoStorage.Get(orderId);
                _orderDto.OnStageChanged += OnStageChanged;   
            }
        }

        private void OnStageChanged(string orderId)
        {
            _orderStateMachine.Switch(_orderDto.Stage.ToString(), _orderDto.OrderID);
        }

        public override void OnExit()
        {
            if (_orderDto != null)
            {
                _orderDto.OnStageChanged -= OnStageChanged; 
            }
            _orderDto = null;
        }
    }
}