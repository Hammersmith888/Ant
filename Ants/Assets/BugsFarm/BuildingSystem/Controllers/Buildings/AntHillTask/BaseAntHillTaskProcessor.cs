using System.Collections.Generic;
using System.Linq;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseAntHillTaskProcessor
    {
        private List<AntHillTaskDto> _tasks;
        protected BaseAntHillTaskProcessor()
        {
            _tasks = new List<AntHillTaskDto>();
        }

        public void Add(AntHillTaskDto taskDto)
        {
            _tasks.Add(taskDto);
        }

        public void UpdateTask(AntHillTaskActionCompletedProtocol protocol)
        {
            foreach (var taskDto in _tasks)
            {
                if(!taskDto.ReferenceModelID.Contains(protocol.ModelID) && taskDto.ReferenceModelID[0] != "Any")
                    continue;
                if (taskDto.TaskType != protocol.TaskType.ToString())
                    continue;
                if(!IsConditionCompleted(protocol, taskDto))
                    continue;

                RefreshAmount(taskDto, true);
            }
        }

        public abstract void RefreshAmount(AntHillTaskDto taskDto, bool add = false);
        protected abstract bool IsConditionCompleted(AntHillTaskActionCompletedProtocol protocol, AntHillTaskDto taskDto);
        
        public void Clear()
        {
            _tasks.Clear();
        }

    }
}