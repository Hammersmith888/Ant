using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UpgradeSystem;
using BugsFarm.UserSystem;
using UniRx;
using Zenject;

namespace BugsFarm.UI
{
    public class BuildingInfoMenuUpgrade : InteractionBaseCommand
    {
        private readonly IUser _user;
        private readonly IUIService _uiService;
        private readonly IInstantiator _instantiator;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private readonly IconLoader _iconLoader;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingUpgradeModelStorage _buildingUpgradeModelStorage;
        private readonly BuildingInfoParamsModelsStorage _buildingInfoParamsModelsStorage;
        private readonly BuildingParamsModelStorage _paramModelStorage;
        private const string _upgradeTextKey = "UIBuildingInfoMenu_UpgradeLabel";
        private BuildingDto _buildingDto;

        public BuildingInfoMenuUpgrade(IUser user,
                                       IUIService uiService,
                                       IInstantiator instantiator,
                                       IBuildingBuildSystem buildingBuildSystem,
                                       IconLoader iconLoader,
                                       BuildingDtoStorage buildingDtoStorage,
                                       StatsCollectionStorage statsCollectionStorage,
                                       BuildingUpgradeModelStorage buildingUpgradeModelStorage,
                                       BuildingInfoParamsModelsStorage buildingInfoParamsModelsStorage,
                                       BuildingParamsModelStorage paramModelStorage)
        {
            _user = user;
            _uiService = uiService;
            _instantiator = instantiator;
            _buildingBuildSystem = buildingBuildSystem;
            _iconLoader = iconLoader;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingUpgradeModelStorage = buildingUpgradeModelStorage;
            _buildingInfoParamsModelsStorage = buildingInfoParamsModelsStorage;
            _paramModelStorage = paramModelStorage;
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
            if (!CanUpgrade(out var price))
            {
                HideUpgrade(_buildingDto.Guid);
                _buildingDto = null;
                return base.Execute(protocol);
            }

            _buildingBuildSystem.OnStarted += HideUpgrade;
            _buildingBuildSystem.OnCompleted += ShowUpgrade;

            window.CloseEvent += (sender, args) =>
            {
                _buildingBuildSystem.OnStarted -= HideUpgrade;
                _buildingBuildSystem.OnCompleted -= ShowUpgrade;
                _buildingDto = null;
            };
            
            window.UpgradeEvent += (sender, args) =>
            {
                if (!_user.HasCurrency(price))
                {
                    window.SetUpgradeButtonInterractable(false);
                    return;
                }
                _user.AddCurrency(price.ModelID, -price.Count);
                _instantiator.Instantiate<UpgradeBuildingCommand>()
                    .Execute(new UpgradeBuildingProtocol(protocol.Guid));
                MessageBroker.Default.Publish(new AntHillTaskActionCompletedProtocol()
                {
                    Guid = protocol.Guid,
                    ModelID = _buildingDtoStorage.Get(protocol.Guid).ModelID,
                    TaskType = AntHillTaskType.Upgrade,
                    EntityType = AntHillTaskReferenceGroup.Building
                });
                window.Close();
            };

            if (_buildingBuildSystem.IsBuilding(_buildingDto.Guid))
            {
                HideUpgrade(_buildingDto.Guid);
            }
            else
            {
                ShowUpgrade(_buildingDto.Guid);
            }
            return base.Execute(protocol);
        }

        private void ShowUpgrade(string buildingId)
        {
            if (_buildingDto == null || buildingId != _buildingDto.Guid)
            {
                return;
            }
            
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            if (!CanUpgrade(out var price))
            {
                HideUpgrade(_buildingDto.Guid);
                return;
            }
            window.SetUpgradeButton(true);
            window.SetUpgradeButtonInterractable(_user.HasCurrency(price));
            window.SetUpgradeCost(price.Count.ToString());
            window.SetUpgradeLabel(LocalizationManager.Localize(_upgradeTextKey));
            window.SetParams(GetParams());
        }

        private void HideUpgrade(string buildingId)
        {
            if (_buildingDto == null || buildingId != _buildingDto.Guid)
            {
                return;
            }
            
            var window = _uiService.Get<UIBuildingInfoMenuWindow>();
            window.SetUpgradeButton(false);
            window.SetParams(GetParams());
        }

        private bool CanUpgrade(out CurrencyModel price)
        {
            price = default;
            if (_buildingDto == null)
            {
                return false;
            }
            if (!_paramModelStorage.HasEntity(_buildingDto.ModelID) ||
                !_paramModelStorage.Get(_buildingDto.ModelID).Params.Contains("Upgrade"))
            {
                return false;
            }

            if (!_buildingUpgradeModelStorage.HasEntity(_buildingDto.ModelID))
            {
                return false;
            }


            var nextLevelValue = 1;
            _instantiator.Instantiate<GetEntityLevelCommand>()
                .Execute(new GetEntitytLevelProtocol(_buildingDto.Guid, result => nextLevelValue += result));

            var upgradeModel = _buildingUpgradeModelStorage.Get(_buildingDto.ModelID);
            if (!upgradeModel.Levels.ContainsKey(nextLevelValue))
            {
                return false;
            }
            price = upgradeModel.Levels[nextLevelValue].Price;
            return true;
        }
        
        private IEnumerable<InfoParamData> GetParams()
        {
            if (_buildingDto == null)
            {
                yield break;
            }
            
            if (_buildingBuildSystem.IsBuilding(_buildingDto.Guid))
            {
                yield break;
            }
            
            if (!_buildingInfoParamsModelsStorage.HasEntity(_buildingDto.ModelID))
            {
                yield break;
            }

            if (!_statsCollectionStorage.HasEntity(_buildingDto.Guid))
            {
                yield break;
            }
            

            var upgradeStats = new Dictionary<string, UpgradeStatModel>();
            if (_buildingUpgradeModelStorage.HasEntity(_buildingDto.ModelID))
            {
                var nextLevel = 1;
                _instantiator.Instantiate<GetEntityLevelCommand>()
                    .Execute(new GetEntitytLevelProtocol(_buildingDto.Guid, result => nextLevel += result));
                var upgradeModel = _buildingUpgradeModelStorage.Get(_buildingDto.ModelID);
                if (upgradeModel.Levels.ContainsKey(nextLevel))
                {
                    upgradeStats = upgradeModel.Levels[nextLevel].Stats.ToDictionary(x=>x.StatID);
                }
            }
            var statCollection = _statsCollectionStorage.Get(_buildingDto.Guid);
            var paramStats = _buildingInfoParamsModelsStorage.Get(_buildingDto.ModelID);
            foreach (var item in paramStats.Items)
            {
                var statId = item.Key;
                var upgradeValue = upgradeStats.ContainsKey(statId) ? (int) upgradeStats[statId].Value : -1;
                var icon = string.IsNullOrEmpty(item.Value.IconName) ? null : _iconLoader.Load(item.Value.IconName);
                yield return new InfoParamData
                {
                    Name = LocalizationManager.Localize(item.Value.LocalizationId),
                    Value = (int) statCollection.GetValue(statId),
                    UpgradeValue = upgradeValue,
                    Icon = icon,
                    FormatId = item.Value.FormatId
                };
            }
        }
    }
}