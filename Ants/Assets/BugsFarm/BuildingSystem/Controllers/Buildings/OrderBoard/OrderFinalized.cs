using BugsFarm.Services.StateMachine;

namespace BugsFarm.BuildingSystem
{
    public class OrderFinalized : State
    {
        private readonly IStateMachine _orderStateMachine;
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly OrderModelsStorage _orderModelsStorage;

        public OrderFinalized(IStateMachine orderStateMachine,
                                   OrderDtoStorage orderDtoStorage,
                                   OrderModelsStorage orderModelsStorage) : base(OrderStage.Finalized.ToString())
        {
            _orderStateMachine = orderStateMachine;
            _orderDtoStorage = orderDtoStorage;
            _orderModelsStorage = orderModelsStorage;
        }

        public override void OnEnter(params object[] args)
        {
            var orderId = (string) args[0];
            if (_orderDtoStorage.HasEntity(orderId))
            {
                var orderModel = _orderModelsStorage.Get(orderId);
                var orderDto = _orderDtoStorage.Get(orderId);
                _orderStateMachine.Switch("Init", orderModel.IsSpecial);
            }
        }
    }
}