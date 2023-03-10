using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class UnitAntHillTaskProcessor : BaseAntHillTaskProcessor
    {
        private Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>> _conditions;

        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly UnitDtoStorage _unitDtoStorage;

        public UnitAntHillTaskProcessor(UnitDtoStorage unitDtoStorage, StatsCollectionStorage statsCollectionStorage)
        {
            _unitDtoStorage = unitDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _conditions = new Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>>()
            {
                {AntHillTaskCondition.None, (x, y) => true},
                {AntHillTaskCondition.Level, IsLevelSufficient},
                {AntHillTaskCondition.Amount, HasAmountOf}
            };
        }


        public override void RefreshAmount(AntHillTaskDto taskDto, bool add = false)
        {
            switch (taskDto.ProgressWay)
            {
                case "MaxLevel":
                    if (taskDto.ReferenceModelID[0] != "Any")
                        taskDto.CurrentValue = GetMaxLevel(taskDto.ReferenceModelID);
                    else
                        taskDto.CurrentValue = GetMaxLevel();
                    break;
                case "Add":
                    taskDto.CurrentValue = Mathf.Min(add ? ++taskDto.CurrentValue : taskDto.CurrentValue, taskDto.CompletionGoal);
                    break;
                case "Amount":
                    taskDto.CurrentValue = GetAmount(taskDto.ReferenceModelID);
                    break;
            }
        }
        private int GetMaxLevel()
        {
            int level = 0;
            foreach (var buildingDto in _unitDtoStorage.Get())
            {
                level = Mathf.Max(level, GetMaxLevel(buildingDto));
            }

            return level;
        }
        private int GetAmount(string[] referenceModelID)
        {
            return _unitDtoStorage.Get().Count(x => referenceModelID.Contains(x.ModelID));
        }

        private int GetMaxLevel(string[] modelID)
        {
            int level = 0;
            for (int i = 0; i < modelID.Length; i++)
            {
                foreach (var unitDto in _unitDtoStorage.Get().Where(x => x.ModelID == modelID[i]))
                {
                    level = Mathf.Max(level, GetMaxLevel(unitDto));
                }
            }

            return level;
        }

        private int GetMaxLevel(UnitDto unitDto)
        {
            if (!_statsCollectionStorage.HasEntity(unitDto.Guid))
                return 0;
            var statsCollection = _statsCollectionStorage.Get(unitDto.Guid);
            if (!statsCollection.HasEntity("stat_level"))
                return 0;
            return Mathf.RoundToInt(statsCollection.GetValue("stat_level"));
        }
        protected override bool IsConditionCompleted(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            return _conditions[taskDto.ConditionName](protocol, taskDto);
        }
        
        private bool IsLevelSufficient(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            if (!_statsCollectionStorage.HasEntity(protocol.Guid))
                return false;
            var statsCollection = _statsCollectionStorage.Get(protocol.Guid);
            if (!statsCollection.HasEntity("stat_level"))
                return false;
            return taskDto.ConditionValue >= statsCollection.GetValue("stat_level");
        }

        private bool HasAmountOf(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            return _unitDtoStorage.Get().Count(x => taskDto.ReferenceModelID.Contains(x.ModelID)) >=
                   taskDto.ConditionValue;
        }
    }
}