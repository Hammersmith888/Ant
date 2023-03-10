using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class GardenCareAssignTask : ITaskAssigner
    {
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly TaskStorage _taskStorage;
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private TaskInfo _taskInfo;

        private const string _gardenCareTaskName = nameof(GardenCareTask);

        public GardenCareAssignTask(SimulatingTeleporter simulatingTeleporter, TaskStorage taskStorage, UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _taskStorage = taskStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _simulatingTeleporter = simulatingTeleporter;
        }

        public bool CanAssign(string guid)
        {
            var unitTaskProcessor = _unitTaskProcessorStorage.Get(guid);
            var allTasks = _taskStorage.GetAllInfo().Where(x => x.TaskName == _gardenCareTaskName);
            
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

        public void Assign(string guid)
        {
            
            var taskGuid = _unitTaskProcessorStorage.Get(guid).GetCurrentTask().Guid;
            var taskInfo = _taskStorage.GetTaskInfo(taskGuid);
            if (taskInfo != null && taskInfo.TaskName == _gardenCareTaskName)
            {
                return;
            }

            var task = _taskStorage.Get(_taskInfo.TaskGuid);

            if (task == null)
                return;
            _simulatingTeleporter.TeleportToConcrete(guid, _taskInfo.OwnerId);
            _unitTaskProcessorStorage.Get(guid).SetTask(task);
        }
    }
}