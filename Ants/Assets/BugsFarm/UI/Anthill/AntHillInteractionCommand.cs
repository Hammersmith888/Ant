using System;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using BugsFarm.UnitSystem;
using BugsFarm.UserSystem;
using BugsFarm.Utility;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.UI
{
    public class AntHillInteractionCommand : State
    {
        public event Action OnCloseWindow;

        public event Action OnTaskListPressed;

        public override string Id => "AntHill";

        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IInputController<SceneLayer> _inputController;

        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly AntHillTaskHandler _antHillTaskHandler;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IActivitySystem _activitySystem;
        private readonly IconLoader _iconLoader;
        private readonly InventoryStorage _inventoryStorage;
        private readonly CompositeDisposable _events;
        private readonly IInstantiator _instantiator;

        private const string _levelStatKey = "stat_level";
        private const string _levelTextKey = "LVL";

        private const string _foodNoNeedTimeStatKey = "stat_noNeedTimeFood";
        private const string _hillUpgradedKey = "UIAntHillTasks_PopUp_Upgraded";
        private const string _foodConsumeStatKey = "stat_consumeFood";
        private const string _bornTimeStatKey = "stat_bornTime";

        private const string _foodItemId = "0";
        private const string _queenModelID = "54";
        private BuildingDto _buildingDto;
        private StatsCollection _statsCollection;
        private UIAntHillWindow _window;
        private InteractionProtocol _protocol;

        public AntHillInteractionCommand(IUser user,
                                         IUIService uiService,
                                         IInputController<SceneLayer> inputController,
                                         StatsCollectionStorage statsCollectionStorage,
                                         BuildingDtoStorage buildingDtoStorage,
                                         UnitDtoStorage unitDtoStorage,
                                         BuildingModelStorage buildingModelStorage,
                                         ISimulationSystem simulationSystem,
                                         IActivitySystem activitySystem,
                                         IconLoader iconLoader,
                                         InventoryStorage inventoryStorage,
                                         IInstantiator instantiator,
                                         AntHillTaskHandler antHillTaskHandler)
        {
            _antHillTaskHandler = antHillTaskHandler;
            _instantiator = instantiator;
            _inventoryStorage = inventoryStorage;
            _simulationSystem = simulationSystem;
            _activitySystem = activitySystem;
            _iconLoader = iconLoader;
            _unitDtoStorage = unitDtoStorage;
            _buildingModelStorage = buildingModelStorage;
            _inputController = inputController;
            _uiService = uiService;
            _user = user;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _events = new CompositeDisposable();
        }

        public void SetDependencies(InteractionProtocol protocol)
        {
            _protocol = protocol;
        }
        
        public override void OnEnter(params object[] arguments)
        {
            if (!_buildingDtoStorage.HasEntity(_protocol.Guid))
            {
                throw new InvalidOperationException();
            }

            _inputController.Lock();
            
            _window = _uiService.Show<UIAntHillWindow>();
            _buildingDto = _buildingDtoStorage.Get(_protocol.Guid);
            _statsCollection = _statsCollectionStorage.Get(_protocol.Guid);
            
            Listen<UserLevelChangedProtocol>(_ => UpdateHeader());
            Listen<DeleteUnitProtocol>(_ => UpdateInfo());
            Listen<DeathUnitProtocol>(_ => UpdateInfo());
            Listen<DeleteBuildingProtocol>(_ => UpdateInfo());


            _window.CloseEvent += CloseWindowButtonPressed;

            _window.UpgradeEvent += OnUpgradedButtonPressed;

            // TODO : remove develop tests
           /* _window.PrisonEvent += (sender, args) =>
            {
                MessageBroker.Default.Publish(new DeathUnitProtocol
                {
                    UnitId = Random.Range(11, 18).ToString(),
                    DeathReason = DeathReason.Fighted
                });
            };*/

            _window.TaskListEvent += TaskListPressed;
            
            UpdateInfo();
            UpdateHeader();
            Observable.EveryUpdate().Subscribe(_ => UpdateDaysPassed()).AddTo(_events);
            var buildingModel = _buildingModelStorage.Get(_buildingDto.ModelID);
            var description = LocalizationHelper.GetBuildingDescription(_buildingDto.ModelID);
            
            UpdateAntHillTaskInfo();
            
            _window.SetIcon(_iconLoader.Load(buildingModel.TypeName));
            _window.SetDescriptionText(description);
        }

        private void OnUpgradedButtonPressed(object sender, EventArgs args)
        {
            _statsCollectionStorage.Get(_user.Id).AddModifier(_levelStatKey, new StatModBaseAdd(1));
            _instantiator.Instantiate<UIUpgradePopUpInteractor>().OpenPopUp();
            UpdateAntHillTaskInfo();
            OnCloseWindow?.Invoke();
        }

        private void TaskListPressed(object sender, EventArgs eventArgs)
        {
            OnTaskListPressed?.Invoke();
        }

        private void CloseWindowButtonPressed(object sender, EventArgs eventArgs)
        {
            OnCloseWindow?.Invoke();
        }
        public override void OnExit()
        {
            _events.Dispose();
            _events.Clear();
            _uiService.Hide<UIAntHillWindow>();
            _inputController.UnLock();
            _window = null;
            _buildingDto = null;
            _statsCollection = null;
        }

        private void UpdateAntHillTaskInfo()
        {
            int amountOfCompletedTasks = _antHillTaskHandler.GetAmountOfCompletedTasks();
            int totalAmountOfTasks = _antHillTaskHandler.GetAmountOfTasks();
            _window.SetProgress((float) amountOfCompletedTasks / totalAmountOfTasks);
            _window.SetProgressText($"{amountOfCompletedTasks} / {totalAmountOfTasks}");
            _window.SetUpgradeIcon(amountOfCompletedTasks >= totalAmountOfTasks);
            //_window.SetUpgradeIcon(amountOfCompletedTasks >= 1);
        }
        
        private void Listen<T>(Action<T> onAction) where T : IProtocol
        {
            MessageBroker.Default.Receive<T>().Subscribe(onAction).AddTo(_events);
        }

        private void UpdateInfo()
        {
            if (_buildingDto == null)
            {
                throw new InvalidOperationException();
            }
            
            var unitDtos = _unitDtoStorage.Get().ToArray();
            var buildingsDtos = _buildingDtoStorage.Get().ToArray();
            var consumablesCount = 0;
            var unitCount = 0;
            var maxFoodInGame = 0f;
            var totalConsumePerIteration = 0f;
            var totalConsumeIterationTime = 0f;

            // инкрементирует общие показатели
            void Consumable(string guid)
            {
                if (_statsCollectionStorage.TryGet(guid, out var satatCollection))
                {
                    if (satatCollection.HasEntity(_foodConsumeStatKey) &&
                        satatCollection.HasEntity(_foodNoNeedTimeStatKey))
                    {
                        totalConsumePerIteration += satatCollection.GetValue(_foodConsumeStatKey);
                        totalConsumeIterationTime += satatCollection.GetValue(_foodNoNeedTimeStatKey);
                        consumablesCount++;
                    }
                }
            }

            // подсчет еды которую выдают постройки
            // королева не является юнитом но, требует подсчет сколько она кушает.
            foreach (var buildingDto in buildingsDtos)
            {
                if (_inventoryStorage.TryGet(buildingDto.Guid, out var inventory))
                {
                    maxFoodInGame += inventory.GetItemSlot(_foodItemId)?.Count ?? 0;
                }

                if (buildingDto.ModelID == _queenModelID)
                {
                    Consumable(buildingDto.Guid);
                }
            }

            // подсчет юнитов которые кушают еду.
            foreach (var unitDto in unitDtos)
            {
                // только активные юниты (не активные - мертвый, лечится итд)
                if (!_activitySystem.IsActive(unitDto.Guid))
                {
                    continue;
                }

                unitCount++;
                Consumable(unitDto.Guid);
            }

            var avgTimeIteration = totalConsumeIterationTime / consumablesCount;
            var avgFoodIteration = totalConsumePerIteration / consumablesCount;
            var foodIterations = maxFoodInGame / avgFoodIteration;
            var avgFoodMaxTime = Format.MinutesToSeconds(Mathf.Max(avgTimeIteration * foodIterations, 0));
            var eatPrediction  = Format.Age(avgFoodMaxTime, true);

            // + 1 : The queen is not a unit, but for the user, the queen is a unit..
            _window.SetUnitsCount($"{unitCount + 1}");
            _window.SetMealLeftCount(eatPrediction);
        }

        private void UpdateHeader()
        {
            var window = _uiService.Get<UIAntHillWindow>();
            var userLevel = _user.GetLevel();
            var buildingName = LocalizationHelper.GetBuildingName(_buildingDto.ModelID);
            var levelText = LocalizationManager.Localize(_levelTextKey);
            window.SetHeaderText(buildingName + " " + userLevel + " " + levelText);
        }

        private void UpdateDaysPassed()
        {
            var daysPassed = _simulationSystem.GameAge - _statsCollection.GetValue(_bornTimeStatKey);
            var daysPassedCount = $"{Format.Age(daysPassed, true)}";
            _window.SetDaysPassedCount(daysPassedCount);
        }
    }
}