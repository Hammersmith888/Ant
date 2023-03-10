using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Quest;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.TypeRegistry;
using BugsFarm.Services.UIService;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class BuildingInteractionCommand : InteractionBaseCommand
    {
        private readonly IInstantiator _instantiator;
        private readonly IUIService _uiService;
        private readonly IInputController<MainLayer> _inputController;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly BuildingParamsModelStorage _paramModelStorage;
        private readonly TypeStorage _typeStorage;
        private readonly StateInfoStorage _stateInfoStorage;
        private readonly ResourceInfoStorage _resourceInfoStorage;
        private readonly ResourceBarInfoStorage _resourceBarInfoStorage;
        private readonly BuildingInfoStorage _buildingInfoStorage;
        private readonly IFreePlaceSystem _freePlaceSystem;
        private readonly IconLoader _iconLoader;
        private const string _buildingNamePrefix = "BuildingsName_";
        private const string _deleteTextKey = "UIBuildingInfoMenu_Remove";
        public BuildingInteractionCommand(IInstantiator instantiator,
                                          IUIService uiService,
                                          IInputController<MainLayer> inputController,
                                          BuildingDtoStorage buildingDtoStorage,
                                          BuildingModelStorage buildingModelStorage,
                                          BuildingParamsModelStorage paramModelStorage,
                                          TypeStorage typeStorage,
                                          StateInfoStorage stateInfoStorage,
                                          ResourceInfoStorage resourceInfoStorage,
                                          ResourceBarInfoStorage resourceBarInfoStorage,
                                          BuildingInfoStorage buildingInfoStorage,
                                          IFreePlaceSystem freePlaceSystem,
                                          IconLoader iconLoader)
        {
            _instantiator = instantiator;
            _uiService = uiService;
            _inputController = inputController;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingModelStorage = buildingModelStorage;
            _paramModelStorage = paramModelStorage;
            _typeStorage = typeStorage;
            _stateInfoStorage = stateInfoStorage;
            _resourceInfoStorage = resourceInfoStorage;
            _resourceBarInfoStorage = resourceBarInfoStorage;
            _buildingInfoStorage = buildingInfoStorage;
            _freePlaceSystem = freePlaceSystem;
            _iconLoader = iconLoader;
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            var disposables = new CompositeDisposable();

            if (!_buildingDtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"Building with Id : {protocol.Guid}" +
                                            $", does not exist");
            }
            var dto = _buildingDtoStorage.Get(protocol.Guid);
            // lock all input world interactions
            _inputController.Lock();

            var model = _buildingModelStorage.Get(dto.ModelID);
            var itemName = "";
            var getNameProtocol = new GetEntityNameProtocol(dto.ModelID, dto.Guid,
                                                            _buildingNamePrefix, res => itemName = res);
            _instantiator.Instantiate<GetEntityNameCommand>().Execute(getNameProtocol);

            // show window
            var window = _uiService.Show<UIBuildingInfoMenuWindow>();
            window.CloseEvent += (o, args) =>
            {
                _uiService.Hide<UIBuildingInfoMenuWindow>();
                _inputController.UnLock();
                disposables.Dispose();
            };

            window.DeleteEvent += (sender, args) =>
            {
                _uiService.Hide<UIBuildingInfoMenuWindow>();
                _inputController.UnLock();
                disposables.Dispose();
                window.HidedEvent += () => OpenYesNoWindow(protocol, model, window, itemName);
            };

            window.MoveEvent += (sender, args) =>
            {
                var modelId = _buildingDtoStorage.Get(protocol.Guid).ModelID;
                var selectPlaceInteractor = _instantiator.Instantiate<SelectPlaceInteractor>();
                selectPlaceInteractor.SelectPlace(modelId, placeNum =>
                {
                    _freePlaceSystem.FreePlace(modelId, placeNum, result =>
                    {
                        if (result)
                        {
                            _instantiator.Instantiate<PlaceBuildingCommand>()
                                .Execute(new PlaceBuildingProtocol(modelId, protocol.Guid, placeNum));
                        }
                    }, protocol.Guid);
                });
                window.Close();
            };
            
            // Restock
            _instantiator.Instantiate<BuildingInfoMenuRestock>().Execute(protocol);
            // Upgrade
            _instantiator.Instantiate<BuildingInfoMenuUpgrade>().Execute(protocol);
            // Speedup
            _instantiator.Instantiate<BuildingInfoMenuSpedup>().Execute(protocol);
            
            // setup window
            window.SetHeader(itemName);
;
            var descText = LocalizationManager.Localize($"BuildingsDescription_{dto.ModelID}");
            var builder = new StringBuilder();
            // Start TODO : Make it beautiful , and remove local field "disposables"
            Observable.EveryUpdate().Subscribe(_ =>
            {
                builder.Clear();
                builder.Append(descText);
                if (_stateInfoStorage.HasEntity(dto.Guid))
                {
                    builder.Append("\n\n");
                    var stateInfo = _stateInfoStorage.Get(dto.Guid);
                    stateInfo.Update();
                    foreach (var infoTxt in stateInfo.Info)
                    {
                        builder.Append(infoTxt + "\n");
                    }

                    window.SetDescription(builder.ToString());
                }
                else
                {
                    window.SetDescription(builder.ToString());
                }

                var hasResourceInfo = _resourceInfoStorage.HasEntity(dto.Guid);
                window.SetResourceActive(hasResourceInfo);
                if (hasResourceInfo)
                {
                    var resourceInfo = _resourceInfoStorage.Get(dto.Guid);
                    resourceInfo.Update();
                    window.SetResourceInfo(resourceInfo.Info);
                }

                var hasResourceBar = _resourceBarInfoStorage.HasEntity(dto.Guid);
                window.SetResourceBar(hasResourceBar);
                window.SetResourceCurrency(hasResourceBar);
                if (hasResourceBar)
                {
                    var resourceBarInfo = _resourceBarInfoStorage.Get(dto.Guid);
                    resourceBarInfo.Update();
                    window.SetResourceBarProgress(resourceBarInfo.Progress);
                    window.SetResourceCurrencyIcon(_iconLoader.LoadCurrency(resourceBarInfo.CurrencyId ?? "0"));
                }

                var hasBuildingBar = _buildingInfoStorage.HasEntity(dto.Guid);
                window.SetBuildingBar(hasBuildingBar);
                if (hasBuildingBar)
                {
                    var buildingInfo = _buildingInfoStorage.Get(dto.Guid);
                    builder.Append("\n\n");
                    builder.Append(buildingInfo.Description);
                    window.SetBuildingProgress(buildingInfo.Progress);
                    window.SetBuildingText(buildingInfo.Info);
                    window.SetDescription(builder.ToString());
                }
            }).AddTo(disposables);
            // End
            
            window.SetIcon(_iconLoader.LoadOrDefault(model.TypeName, model.TypeID));
            window.SetDeleteButton(CanDelete(dto.ModelID));
            window.SetMoveButton(CanMove(dto.ModelID));
            
            return Task.CompletedTask;
        }

        private void OpenYesNoWindow(InteractionProtocol protocol, BuildingModel model, UIBuildingInfoMenuWindow window,
            string itemName)
        {
            var yesNo = _uiService.Show<UIYesNoWindow>();
            yesNo.YesEvent += (o, e) =>
            {
                MessageBroker.Default.Publish(new QuestUpdateProtocol()
                {
                    QuestType = QuestType.RemoveBuilding,
                    ReferenceID = model.ModelID,
                    Value = 1
                });

                _uiService.Hide<UIYesNoWindow>();
                _instantiator.Instantiate<DeleteBuildingCommand>()
                    .Execute(new DeleteBuildingProtocol(protocol.Guid));
                window.Close();
            };
            yesNo.NoEvent += (o, e) => { _uiService.Hide<UIYesNoWindow>(); };
            yesNo.CloseEvent += (o, e) => { _uiService.Hide<UIYesNoWindow>(); };
            yesNo.SetText(string.Format(LocalizationManager.Localize(_deleteTextKey), itemName));
        }

        private bool CanDelete(string modelId)
        {
            return _paramModelStorage.HasEntity(modelId) && 
                   _paramModelStorage.Get(modelId).Params.Any(x=> x == "Delete");
        }

        private bool CanMove(string modelId)
        {
            return _paramModelStorage.HasEntity(modelId) && 
                   _paramModelStorage.Get(modelId).Params.Contains("Move");
        }
    }
}