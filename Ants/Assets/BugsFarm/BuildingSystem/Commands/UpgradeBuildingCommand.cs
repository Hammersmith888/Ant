using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.StatsService;
using UniRx;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class UpgradeBuildingCommand : ICommand<UpgradeBuildingProtocol>
    {
        private readonly BuildingUpgradeModelStorage _buildingUpgradeModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IBuildingBuildSystem _buildingBuildSystem;
        private const string _levelStatKey = "stat_level";

        public UpgradeBuildingCommand(BuildingUpgradeModelStorage buildingUpgradeModelStorage,
                                      BuildingDtoStorage buildingDtoStorage,
                                      StatsCollectionStorage statsCollectionStorage,
                                      IBuildingBuildSystem buildingBuildSystem)
        {
            _buildingUpgradeModelStorage = buildingUpgradeModelStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingBuildSystem = buildingBuildSystem;
        }

        public Task Execute(UpgradeBuildingProtocol protocol)
        {
            if (!_buildingDtoStorage.HasEntity(protocol.BuildingId))
            {
                throw new InvalidOperationException($"Building with Id : {protocol.BuildingId}" +
                                                    $", does not exist");
            }

            if (!_statsCollectionStorage.HasEntity(protocol.BuildingId))
            {
                throw new InvalidOperationException($"Building with Id : {protocol.BuildingId}" +
                                                    $", does not exist stats");
            }
            
            var buildingDto = _buildingDtoStorage.Get(protocol.BuildingId);
            var statCollection = _statsCollectionStorage.Get(protocol.BuildingId);
            var modelId = buildingDto.ModelID;


            if (!statCollection.HasEntity(_levelStatKey))
            {
                throw new InvalidOperationException($"Building with Id : {protocol.BuildingId}" +
                                                    $", does not exist Stat Level");
            }

            if (!_buildingUpgradeModelStorage.HasEntity(modelId))
            {
                throw new InvalidOperationException($"Building with Id : {protocol.BuildingId}" +
                                                    $", does not exist {nameof(BuildingUpgradeModel)}");
            }
            var levelValue = statCollection.GetValue(_levelStatKey);
            var upgradeModel = _buildingUpgradeModelStorage.Get(modelId);
            var nextLevelValue = (int) levelValue + 1;
            if (!upgradeModel.Levels.ContainsKey(nextLevelValue))
            {
                throw new CheckoutException($"UpgradeLevel with [ Next Level : {nextLevelValue} ] does not exist!");
            }
            statCollection.AddModifier(_levelStatKey,new StatModBaseAdd(1));
            var upgradeLevel = upgradeModel.Levels[nextLevelValue];
            foreach (var upstat in upgradeLevel.Stats)
            {
                if (!statCollection.HasEntity(upstat.StatID))
                {
                    Debug.LogError($"Building with modelId : {modelId} does not have Stat : {upstat.StatID}");
                    continue;
                }
                statCollection.AddModifier(upstat.StatID, new StatModBaseAdd(upstat.Value));
            }

            if (_buildingBuildSystem.HasEntity(protocol.BuildingId))
            {
                _buildingBuildSystem.ResetBuildingTime(protocol.BuildingId);
                _buildingBuildSystem.Start(protocol.BuildingId);
            }

            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
    }
}