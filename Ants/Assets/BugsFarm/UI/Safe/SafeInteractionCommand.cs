using System;
using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.UI
{
    public class SafeInteractionCommand : InteractionBaseCommand
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly ICurrencyCollectorSystem _currencyCollectorSystem;
        private readonly IInstantiator _instantiator;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IUIService _uiService;
        private readonly IUser _user;

        private const string _resourceStatKey = "stat_resource";
        private const string _currencyIDStatKey = "stat_currencyID";
        private const string _takeTimeStatKey = "stat_takeTime";
        private StatsCollection _statsCollection;
        private InteractionProtocol _interactionProtocol;
        private StatVital _resourceStat;
        private StatVital _takeTimerStat;
        private UISafeWindow _window;
        private ITask _timerTask;

        public SafeInteractionCommand(StatsCollectionStorage statsCollectionStorage,
                                      ICurrencyCollectorSystem currencyCollectorSystem,
                                      IInstantiator instantiator,
                                      IInputController<MainLayer> inputController,
                                      IUIService uiService,
                                      IUser user)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _currencyCollectorSystem = currencyCollectorSystem;
            _instantiator = instantiator;
            _inputController = inputController;
            _uiService = uiService;
            _user = user;
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            _inputController.Lock();
            _interactionProtocol = protocol;
            _interactionProtocol.ObjectType = SceneObjectType.Building;
            _statsCollection = _statsCollectionStorage.Get(protocol.Guid);
            _resourceStat = _statsCollection.Get<StatVital>(_resourceStatKey);
            _takeTimerStat = _statsCollection.Get<StatVital>(_takeTimeStatKey);
            _takeTimerStat.OnCurrentValueChanged += OnTakeTimeCurrentValueChanged;
            _resourceStat.OnCurrentValueChanged += OnResourceCurrentValueChanged;
            _window = _uiService.Show<UISafeWindow>();
            _window.InfoEvent += (sender, args) =>
            {
                _window.Close();
                _instantiator.Instantiate<InteractionCommand>().Execute(_interactionProtocol);
            };

            _window.CloseEvent += (sender, args) =>
            {
                Dispose();
            };

            _window.BuyEvent += (sender, args) =>
            {
                var currencyId = ((int) _statsCollection.GetValue(_currencyIDStatKey)).ToString();
                var currencyValue = (int) _resourceStat.CurrentValue;
                _currencyCollectorSystem.Collect(_window.CurrencyTarget,currencyId,currencyValue,false, left =>
                {
                    var collected = currencyValue - left;
                    currencyValue = left;
                    _user.AddCurrency(currencyId, collected);
                });
                _resourceStat.CurrentValue = 0;
                _window.Close();
            };

            Update();
            return Task.CompletedTask;
        }

        private void Update()
        {
            if (!_window)
            {
                return;
            }

            var isFull = _resourceStat.CurrentValue >= _resourceStat.Value;
            _window.SetCountMaxText(((int) _resourceStat.Value).ToString());
            _window.SetBuyButtonActive(isFull);
            _window.SetPriceText("250 руб.");

            _window.SetProgressActive(true);
            _window.SetProgress(_resourceStat.CurrentValue / _resourceStat.Value);
            _window.SetProgressText(Format.Resource(_resourceStat.CurrentValue, _resourceStat.Value, true));

            _window.SetStateActive(isFull);
            _window.SetTimerActive(isFull);
        }

        private void Dispose()
        {
            _inputController.UnLock();
            _uiService.Hide<UISafeWindow>();
            _resourceStat.OnCurrentValueChanged -= OnResourceCurrentValueChanged;
            _window = null;
        }

        private void OnResourceCurrentValueChanged(object sender, EventArgs e)
        {
            Update();
        }

        private void OnTakeTimeCurrentValueChanged(object sender, EventArgs e)
        {
            if (!_window)
            {
                return;
            }

            _window.SetTimerText(Format.Time(TimeSpan.FromMinutes(_takeTimerStat.CurrentValue)));
        }
    }
}