using System;
using System.ComponentModel.Design;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.StatsService;
using UniRx;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UpgradeUnitCommand : ICommand<UpgradeUnitProtocol>
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitUpgradeModelStorage _unitUpgradeModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _levelStatKey = "stat_level";
        private const string _xpStatKey = "stat_xp";

        public UpgradeUnitCommand(UnitDtoStorage unitDtoStorage,
                                  UnitUpgradeModelStorage unitUpgradeModelStorage,
                                  StatsCollectionStorage statsCollectionStorage)
        {
            _unitDtoStorage = unitDtoStorage;
            _unitUpgradeModelStorage = unitUpgradeModelStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public Task Execute(UpgradeUnitProtocol protocol)
        {
            if (!_unitDtoStorage.HasEntity(protocol.UnitId))
            {
                throw new InvalidOperationException($"Unit with Id : {protocol.UnitId}" +
                                                    $", does not exist");
            }

            if (!_statsCollectionStorage.HasEntity(protocol.UnitId))
            {
                throw new InvalidOperationException($"Unit with Id : {protocol.UnitId}" +
                                                    $", does not exist stats");
            }
            
            var unitDto = _unitDtoStorage.Get(protocol.UnitId);
            var statCollection = _statsCollectionStorage.Get(protocol.UnitId);
            var modelId = unitDto.ModelID;


            if (!statCollection.HasEntity(_levelStatKey))
            {
                throw new InvalidOperationException($"Unit with Id : {protocol.UnitId}" +
                                                    $", does not exist Stat Level");
            }

            if (!_unitUpgradeModelStorage.HasEntity(modelId))
            {
                throw new InvalidOperationException($"Unit with Id : {protocol.UnitId}" +
                                                    $", does not exist {nameof(UnitUpgradeModel)}");
            }
            var levelValue = statCollection.GetValue(_levelStatKey);
            var upgradeModel = _unitUpgradeModelStorage.Get(modelId);
            var nextLevelValue = (int) levelValue + 1;
            if (!upgradeModel.Levels.ContainsKey(nextLevelValue))
            {
                throw new CheckoutException($"UpgradeLevel with [ Next Level : {nextLevelValue} ] does not exist!");
            }
            statCollection.AddModifier(_levelStatKey,new StatModBaseAdd(1));
            var upgradeLevel = upgradeModel.Levels[nextLevelValue];
            var xpStat = statCollection.Get<StatVital>(_xpStatKey);
            xpStat.CurrentValue = 0;
            foreach (var upstat in upgradeLevel.Stats)
            {
                if (!statCollection.HasEntity(upstat.StatID))
                {
                    Debug.LogError($"Unit with modelId : {modelId} does not have Stat : {upstat.StatID}");
                    continue;
                }
                statCollection.AddModifier(upstat.StatID, new StatModBaseAdd(upstat.Value));
            }

            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
    }
}