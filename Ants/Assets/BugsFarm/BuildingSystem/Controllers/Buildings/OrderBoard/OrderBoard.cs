using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StateMachine;
using BugsFarm.UI;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class OrderBoard : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IStateMachine _baseOrder;
        private readonly IStateMachine _specialOrder;
        private readonly IStateMachine _dealerProcess;
        private readonly IInstantiator _instantiator;
        private const string _orderItemId = "6";
        private OrderDto _specialOrderDto;
        private BuildingSceneObject _view;
        private bool _finalized;

        public OrderBoard(string guid,
                          IStateMachine baseOrder,
                          IStateMachine specialOrder,
                          IStateMachine dealerProcess,
                          IInstantiator instantiator)
        {
            Id = guid;
            _baseOrder = baseOrder;
            _specialOrder = specialOrder;
            _dealerProcess = dealerProcess;
            _instantiator = instantiator;
        }

        public void Initialize()
        {
            if (_finalized) return;
            var inventoryProtocol = new CreateInventoryProtocol(Id, new ItemSlot(_orderItemId,0,2));
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            InitOrder(_baseOrder, false);
            InitOrder(_specialOrder, true);
            InitDealer(_dealerProcess);
        }

        public void Dispose()
        {
            if (_finalized) return;
            _baseOrder.Clear();
            _specialOrder.Clear();
            _dealerProcess.Clear();
            _finalized = true;
        }

        private void InitOrder(IStateMachine orderStateMachine, bool isSpecial)
        {
            if (orderStateMachine.Any())
            {
                return;
            }

            var args = new object[] {orderStateMachine};
            var argsExtend = new object[] {orderStateMachine, Id};
            orderStateMachine.Add(_instantiator.Instantiate<OrderInit>(argsExtend));
            orderStateMachine.Add(_instantiator.Instantiate<OrderProcess>(args));
            orderStateMachine.Add(_instantiator.Instantiate<OrderReward>(args));
            orderStateMachine.Add(_instantiator.Instantiate<OrderRewarding>(args));
            orderStateMachine.Add(_instantiator.Instantiate<OrderWaitNextOrder>(args));
            orderStateMachine.Add(_instantiator.Instantiate<OrderFinalized>(args));
            orderStateMachine.Switch("Init", isSpecial);
        }

        private void InitDealer(IStateMachine delerStateMachine)
        {
            if (delerStateMachine.Any())
            {
                return;
            }

            var args = new object[] {delerStateMachine};
            delerStateMachine.Add(_instantiator.Instantiate<DealerInit>(args));
            delerStateMachine.Add(_instantiator.Instantiate<DealerProcess>(new object[] {Id}));
            delerStateMachine.Switch("Init");
        }
    }
}