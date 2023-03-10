using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class RepairTaskAssigner : ITaskAssigner
    {
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly BuildingBuildSystem _buildingBuildSystem;
        private readonly SimulatingTeleporter _simulatingTeleporter;
        private readonly TaskStorage _taskStorage;
        private TaskInfo _taskInfo;

        private const string _buildingTaskName = nameof(BuildingBootstrapTask);
        
        public RepairTaskAssigner(BuildingBuildSystem buildingBuildSystem, SimulatingTeleporter simulatingTeleporter, TaskStorage taskStorage, UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _taskStorage = taskStorage;
            _simulatingTeleporter = simulatingTeleporter;
            _buildingBuildSystem = buildingBuildSystem;
        }

        public bool CanAssign(string guid)
        {
            var taskGuid = _unitTaskProcessorStorage.Get(guid).GetCurrentTask().Guid;
            var taskInfo = _taskStorage.GetTaskInfo(taskGuid);
            if (taskInfo != null && taskInfo.TaskName == _buildingTaskName)
            {
                return true;
            }

            return false;
        }

        public void Assign(string guid)
        {
            var taskGuid = _unitTaskProcessorStorage.Get(guid).GetCurrentTask().Guid;
            var taskInfo = _taskStorage.GetTaskInfo(taskGuid);
            if (taskInfo != null && taskInfo.TaskName == _buildingTaskName)
            {
                _simulatingTeleporter.TeleportToConcrete(guid, taskInfo.OwnerId);
                return;
            }
            var task = _taskStorage.Get(_taskInfo.TaskGuid);

            if (task == null)
                return;
            _unitTaskProcessorStorage.Get(guid).SetTask(task);
            _simulatingTeleporter.TeleportToConcrete(guid, _taskInfo.OwnerId);
        }
    }
}