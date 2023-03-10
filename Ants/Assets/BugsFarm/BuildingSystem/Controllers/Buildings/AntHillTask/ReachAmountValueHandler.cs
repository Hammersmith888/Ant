using System;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class ReachAmountValueHandler : IAntHillTaskValueHandler
    {
        private readonly Func<AntHillTaskDto, int> _countAmountFunc;

        public ReachAmountValueHandler(Func<AntHillTaskDto, int> countAmountFunc)
        {
            _countAmountFunc = countAmountFunc;
        }

        public void AddAmountTo(AntHillTaskDto antHillTaskDto)
        {
            // int currentAmount = _buildingDtoStorage.Get().Count(x => x.ModelID == antHillTaskDto.ReferenceModelID);
            int currentAmount = _countAmountFunc(antHillTaskDto);
            antHillTaskDto.CurrentValue = Mathf.Min(currentAmount, antHillTaskDto.CompletionGoal);
        }
    }
}