using System;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class FarmShopState : State
    {
        private readonly IStateMachine _stateMachine;
        private readonly IInputController<MainLayer> _inputController;
        private readonly IInstantiator _instantiator;
        private readonly BuildingShopItemModelStorage _buildingShopItemModelStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly IUIService _uiService;
        private readonly IconLoader _iconLoader;
        private readonly IUser _user;
        private readonly IFreePlaceSystem _freePlaceSystem;

        private const string _buildingNameKey = "BuildingsName_";
        private const string _buildingDiscriptionKey = "BuildingsDescription_";
        private FarmShopInteractor _farmShopInteractor;
        private IDisposable _userLevelEvent;
        private int _lastTabIndex;
        
        public FarmShopState(IStateMachine stateMachine,
                             IInputController<MainLayer> inputController, 
                             IFreePlaceSystem freePlaceSystem,
                             IInstantiator instantiator,
                             BuildingShopItemModelStorage buildingShopItemModelStorage,
                             BuildingModelStorage buildingModelStorage,
                             IUIService uiService,
                             IconLoader iconLoader,
                             IUser user) : base("FarmShop")
        {
            _stateMachine = stateMachine;
            _inputController = inputController;
            _instantiator = instantiator;
            _buildingShopItemModelStorage = buildingShopItemModelStorage;
            _buildingModelStorage = buildingModelStorage;
            _uiService = uiService;
            _iconLoader = iconLoader;
            _user = user;
            _freePlaceSystem = freePlaceSystem;
        }

        public override void OnEnter(params object[] args)
        {
            _inputController.Lock();
            _farmShopInteractor = _instantiator.Instantiate<FarmShopInteractor>();
            _userLevelEvent = MessageBroker.Default.Receive<UserLevelChangedProtocol>()
                .Subscribe(_ => OnUserLevelChangedEvent());
            var window = _uiService.Get<UIFarmShopWindow>();
            window.CloseEvent += (sender, e) =>
            {
                _uiService.Hide<UIFarmShopWindow>();
                _stateMachine.Exit(Id);
            };
            
            _farmShopInteractor.OnCellClicked += modelId =>
            {
                var shopItemInfoWindow = _uiService.Show<UIFarmShopItemInfoWindow>();
                var shopItemModel = _buildingShopItemModelStorage.Get(modelId);
                var buildingModel = _buildingModelStorage.Get(modelId);
                shopItemInfoWindow.SetHeader(LocalizationManager.Localize(_buildingNameKey + modelId));
                shopItemInfoWindow.SetDescription(LocalizationManager.Localize(_buildingDiscriptionKey + modelId));
                shopItemInfoWindow.SetIcon(_iconLoader.LoadOrDefault(buildingModel.TypeName, buildingModel.TypeID));
                shopItemInfoWindow.SetItemPrice(shopItemModel.Price.Count.ToString());

                shopItemInfoWindow.CloseEvent += (sender, eventArgs) =>
                {
                    _uiService.Hide<UIFarmShopItemInfoWindow>();
                };
                
                shopItemInfoWindow.ConfirmEvent += (sender, eventArgs) =>
                {
                    shopItemInfoWindow.Close();
                    OnBuyEventHandler(modelId);
                };
            };
            
            _farmShopInteractor.OnCellBuyClicked += modelId =>
            {
                // hide UI
               OnBuyEventHandler(modelId);
            };

            _farmShopInteractor.OnPageSwitch += tabIndex => _lastTabIndex = tabIndex;
            _farmShopInteractor.Init(_lastTabIndex);
        }

        private void OnBuyEventHandler(string modelId)
        {
            var shopModel = _buildingShopItemModelStorage.Get(modelId);
            
            _uiService.Hide<UIFarmShopWindow>();
            _inputController.UnLock();

            if (!_user.HasCurrency(shopModel.Price))
            {
                return;
            }
            

            var selectPlaceInteractor = _instantiator.Instantiate<SelectPlaceInteractor>();
            selectPlaceInteractor.SelectPlace(modelId, placeNum =>
            {
                _freePlaceSystem.FreePlace(modelId, placeNum, result =>
                {
                    if (result && _user.HasCurrency(shopModel.Price))
                    {
                        MessageBroker.Default.Publish(new AntHillTaskActionCompletedProtocol()
                        {
                            ModelID = modelId, EntityType = AntHillTaskReferenceGroup.Building, TaskType = AntHillTaskType.Build, Guid = Id
                        });
                        
                        _farmShopInteractor.BuySuccess(modelId);
                        var protocol = new CreateBuildingProtocol(modelId, placeNum, true);
                        _instantiator.Instantiate<CreateBuildingCommand>().Execute(protocol);
                    }
                    _stateMachine.Exit(Id);
                });
            });
        }

        private void OnUserLevelChangedEvent()
        {
            _farmShopInteractor.SelectTabElement(_lastTabIndex);
        }

        public override void OnExit()
        {
            _userLevelEvent?.Dispose();
            _userLevelEvent = null;
            _farmShopInteractor?.Dispose();
            _farmShopInteractor = null;
            _inputController.UnLock();
            _uiService.Get<UIFarmShopItemInfoWindow>().Close();
        }
    }
}