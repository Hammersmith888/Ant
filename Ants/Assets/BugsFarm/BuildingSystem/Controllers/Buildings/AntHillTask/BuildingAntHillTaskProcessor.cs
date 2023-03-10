
using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BuildingAntHillTaskProcessor : BaseAntHillTaskProcessor
    {
        private Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>> _conditions;

        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;

        public BuildingAntHillTaskProcessor(StatsCollectionStorage statsCollectionStorage, BuildingSceneObjectStorage buildingSceneObjectStorage, BuildingDtoStorage buildingDtoStorage)
        {
            _buildingDtoStorage = buildingDtoStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _conditions = new Dictionary<string, Func<AntHillTaskActionCompletedProtocol, AntHillTaskDto, bool>>()
            {
                {AntHillTaskCondition.None, (x, y) => true},
                {AntHillTaskCondition.Level, IsLevelSufficient},
                {AntHillTaskCondition.Floor, IsBuiltOnFloor},
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

        private int GetAmount(string[] referenceModelID)
        {
            return _buildingDtoStorage.Get().Count(x => referenceModelID.Contains(x.ModelID));
        }

        private int GetMaxLevel(string[] modelID)
        {
            
            int level = 0;
            for (int i = 0; i < modelID.Length; i++)
            {
                foreach (var buildingDto in _buildingDtoStorage.Get().Where(x => x.ModelID == modelID[i]))
                {
                     level = Mathf.Max(level, GetMaxLevel(buildingDto));
                }
            }

            return level;
        }

        private int GetMaxLevel(BuildingDto buildingDto)
        {
            if (!_statsCollectionStorage.HasEntity(buildingDto.Guid))
                return 0;
            var statsCollection = _statsCollectionStorage.Get(buildingDto.Guid);
            if (!statsCollection.HasEntity("stat_level"))
                return 0;
            return Mathf.RoundToInt(statsCollection.GetValue("stat_level"));
        }

        private int GetMaxLevel()
        {
            int level = 0;
            foreach (var buildingDto in _buildingDtoStorage.Get())
            {
                level = Mathf.Max(level, GetMaxLevel(buildingDto));
            }

            return level;
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
        private bool IsBuiltOnFloor(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            if (!_buildingSceneObjectStorage.HasEntity(protocol.Guid))
                return false;
            BuildingSceneObject sceneObject = _buildingSceneObjectStorage.Get(protocol.Guid);
            if (sceneObject.transform.position.y > 13.0f)
                return true;
            return false;
        }
        private bool HasAmountOf(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto)
        {
            return _buildingDtoStorage.Get().Count(x => taskDto.ReferenceModelID.Contains(x.ModelID)) >=
                   taskDto.ConditionValue;
        }
        
    }
}