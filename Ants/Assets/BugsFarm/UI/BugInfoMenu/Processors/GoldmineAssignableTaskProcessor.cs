using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.UI
{
    public class GoldmineAssignableTaskProcessor : IUnitAssignableTaskProcessor
    {
        public TaskInfo TaskInfo => _taskInfo;
        
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly TaskStorage _taskStorage;
        private TaskInfo _taskInfo;

        private const string _goldmineModelID = "44";
        private const string _goldmineTaskName = "GoldmineBootstrapTask";
        
        public GoldmineAssignableTaskProcessor(BuildingDtoStorage buildingDtoStorage,
            TaskStorage taskStorage,
            UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _taskStorage = taskStorage;
            _buildingDtoStorage = buildingDtoStorage;
        }

        public bool CanExecute(string guid)
        {
            int count = _buildingDtoStorage.Get().Count(x => x.ModelID == _goldmineModelID);
            if (count == 0)
            {
                return false;
            }
            var unitTaskProcessor = _unitTaskProcessorStorage.Get(guid);

            var allTasks = _taskStorage.GetAllInfo().Where(x => x.TaskName == _goldmineTaskName);
            
            foreach (var taskInfo in allTasks)
            {
                var task = _taskStorage.Get(taskInfo.TaskGuid);
                if (unitTaskProcessor.CanExecute(task))
                {
                    _taskInfo = taskInfo;
                    return true;
                }
            }

            return false;
        }

        public void Execute(string guid)
        {
            var taskGuid = _unitTaskProcessorStorage.Get(guid).GetCurrentTask().Guid;
            var taskInfo = _taskStorage.GetTaskInfo(taskGuid);
            if (taskInfo != null && taskInfo.TaskName == _goldmineTaskName)
            {
                return;
            }
            var task = _taskStorage.Get(_taskInfo.TaskGuid);

            if (task == null)
                return;
            
            _unitTaskProcessorStorage.Get(guid).SetTask(task);
        }
    }
}