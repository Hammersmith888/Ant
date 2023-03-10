using System.Linq;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using BugsFarm.UserSystem;

namespace BugsFarm.BuildingSystem
{
    public class OrderRewarding : State
    {
        private readonly OrderDtoStorage _orderDtoStorage;
        private readonly IStateMachine _orderStateMachine;
        private readonly ICurrencyCollectorSystem _currencyCollectorSystem;
        private readonly IUIService _uiService;
        private readonly IUser _user;
        private OrderDto _orderDto;
        private int _completeCount;

        public OrderRewarding(IUser user,
                              IUIService uiService,
                              IStateMachine orderStateMachine,
                              ICurrencyCollectorSystem currencyCollectorSystem,
                              OrderDtoStorage orderDtoStorage) : base(OrderStage.Rewarding.ToString())
        {
            _user = user;
            _uiService = uiService;
            _orderStateMachine = orderStateMachine;
            _currencyCollectorSystem = currencyCollectorSystem;
            _orderDtoStorage = orderDtoStorage;
        }

        public override void OnEnter(params object[] args)
        {
            var orderId = (string) args[0];
            if (_orderDto != null)
            {
                return;
            }

            _orderDto = _orderDtoStorage.Get(orderId);
            var window = _uiService.Get<UIOrderBoard>();
            if (window.gameObject.activeSelf)
            {
                VisibleCollect();
            }
            else
            {
                PostLoadCollect();
            }
        }

        public override void OnExit()
        {
            _orderDto = null;
        }

        private void PostLoadCollect()
        {
            foreach (var rewardItem in _orderDto.Rewards)
            {
                _user.AddCurrency(rewardItem.Key, (int)rewardItem.CurrentValue);
                rewardItem.CurrentValue = 0;
            }

            _completeCount = 0;
            OnCurrencyCollected();
        }

        private void VisibleCollect()
        {
            var window = _uiService.Get<UIOrderBoard>();
            var currencyItems = window.GetComponentsInChildren<OrderCurrencyItem>()
                .Where(x => x.OrderID == _orderDto.OrderID)
                .ToDictionary(x => x.ID);
            _completeCount = currencyItems.Count - 1;

            foreach (var currency in _orderDto.Rewards)
            {
                var currencyItem = currencyItems[currency.Key];
                var position = currencyItem.Position;
                _currencyCollectorSystem.Collect(position,
                                                 currency.Key,
                                                 (int) currency.CurrentValue,
                                                 false,
                                                 currencyLeft =>
                                                 {
                                                     currencyItem.SetText(currencyLeft.ToString());
                                                     _user.AddCurrency(currency.Key, (int)(currency.CurrentValue - currencyLeft));
                                                     currency.CurrentValue = currencyLeft;
                                                 },
                                                 OnCurrencyCollected);
            }
        }

        private void OnCurrencyCollected()
        {
            if (_completeCount > 0)
            {
                _completeCount--;
                return;
            }

            _orderDto.Stage = OrderStage.WaitNextOrder;
            _orderStateMachine.Switch(_orderDto.Stage.ToString(), _orderDto.OrderID);
        }
    }
}