using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.UI
{
    public class BuildingInfoMenuSpedup : InteractionBaseCommand
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingParamsModelStorage _paramModelStorage;
        private readonly BuildingSpeedupModelStorage _buildingSpeedupModelStorage;
        private readonly BuildingUpgradeModelStorage _buildingUpgradeModelStorage;
        private const string _speedupTextKey = "UIBuildingInfoMenu_SpeedupLabel";
        private BuildingSpeedupModel _speedupModel;
        private BuildingDto _buildingDto;

        public BuildingInfoMenuSpedup(IUser user,
                                      IUIService uiService,
                                      IInstantiator instantiator,
                                      IBuildingBuildSystem buildingBuildSystem,
                                      BuildingDtoStorage buildingDtoStorage,
                                      BuildingParamsModelStorage paramModelStorage,
                                      BuildingSpeedupModelStorage buildingSpeedupModelStorage,
                                      BuildingUpgradeModelStorage buildingUpgradeModelStorage)
        {
            _user = user;
            _uiService = uiService;
            _instantiator = instantiator;
            _buildingBuildSystem = buildingBuildSystem;
            _buildingDtoStorage = buildingDtoStorage;
            _paramModelStorage = paramModelStorage;
            _buildingSpeedupModelStorage = buildingSpeedupModelStorage;
            _buildingUpgradeModelStorage = buildingUpgradeModelStorage;
        }

        public override Task Execute(InteractionProtocol protocol)
        {
            if (!_buildingDtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"Building with Id : {protocol.Guid}" +
                                            $", does not exist");
            }
            
            _buildingDto = _buildingDtoStorage.Get(protocol.Guid);
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            if (!CanSpeedup())
            {
                HideSpeedup(protocol.Guid);
                _buildingDto = null;
                return base.Execute(protocol);
            }

            _speedupModel = _buildingSpeedupModelStorage.Get(_buildingDto.ModelID);
            _buildingBuildSystem.OnStarted += ShowSpeedup;
            _buildingBuildSystem.OnCompleted += HideSpeedup;

            window.CloseEvent += (sender, args) =>
            {
                HideSpeedup(protocol.Guid);
                _buildingBuildSystem.OnStarted -= ShowSpeedup;
                _buildingBuildSystem.OnCompleted -= HideSpeedup;
                _buildingDto = null;
            };
            
            window.SpeedupEvent += (sender, args) =>
            {
                if (_user.HasCurrency(_speedupModel.Price))
                {
                    HideSpeedup(protocol.Guid);
                    _instantiator.Instantiate<SpeedUpBuildingCommand>()
                        .Execute(new SpeedUpBuildingProtocol(protocol.Guid));
                    window.Close();
                }
                else
                {
                    window.SetSpeedupButtonInterractable(false);
                }
            };
            
            if (_buildingBuildSystem.IsBuilding(protocol.Guid))
            {
                ShowSpeedup(protocol.Guid);
            }
            else
            {
                HideSpeedup(protocol.Guid);
            }
            return base.Execute(protocol);
        }

        private void ShowSpeedup(string buildingId)
        {
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            window.SetSpeedupButton(GetPrice(out var price));
            window.SetSpeedupCost(price.Count.ToString());
            window.SetSpeedupButtonInterractable(_user.HasCurrency(price));
            window.SetSpeedupLabel(LocalizationManager.Localize(_speedupTextKey));
        }

        private void HideSpeedup(string buildingId)
        {
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            window.SetSpeedupButton(false);
        }

        private bool GetPrice(out CurrencyModel price)
        {
            price = default;
            if (!CanSpeedup())
            {
                return false;
            }

            if (!_buildingBuildSystem.GetTime(_buildingDto.Guid, out var currTime, out var maxTime))
            {
                return false;
            }
            
            price = _buildingSpeedupModelStorage.Get(_buildingDto.ModelID).Price;
            if (_buildingUpgradeModelStorage.HasEntity(_buildingDto.ModelID))
            {
                var level = 0;
                var getLevelProtocol = new GetEntitytLevelProtocol(_buildingDto.Guid, res => level = res);
                _instantiator.Instantiate<GetEntityLevelCommand>().Execute(getLevelProtocol);

                var upgradeModel = _buildingUpgradeModelStorage.Get(_buildingDto.ModelID);
                if (upgradeModel.Levels.ContainsKey(level))
                {
                    price = upgradeModel.Levels[level].Price;
                }
            }
            price.Count = (int)(price.Count * (currTime / maxTime));
            return true;
        }
        
        private bool CanSpeedup()
        {
            if (_buildingDto == null)
            {
                return false;
            }
            
            if (!_buildingSpeedupModelStorage.HasEntity(_buildingDto.ModelID))
            {
                return false;
            }
            
            if (!_paramModelStorage.HasEntity(_buildingDto.ModelID))
            {
                return false;
            }
            
            var praramModel = _paramModelStorage.Get(_buildingDto.ModelID);
            return praramModel.Params.Contains("Speedup");
        }
    }
}