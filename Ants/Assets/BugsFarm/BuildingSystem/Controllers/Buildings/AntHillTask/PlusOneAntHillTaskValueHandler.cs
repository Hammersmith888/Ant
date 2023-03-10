using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class PlusOneAntHillTaskValueHandler : IAntHillTaskValueHandler
    {
        public void AddAmountTo(AntHillTaskDto taskDto)
        {
            taskDto.CurrentValue = Mathf.Min(++taskDto.CurrentValue, taskDto.CompletionGoal);
        }
    }
}