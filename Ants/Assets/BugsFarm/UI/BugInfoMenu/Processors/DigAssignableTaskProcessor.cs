using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.RoomSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.UI
{
    public class DigAssignableTaskProcessor : IUnitAssignableTaskProcessor
    {
        public TaskInfo TaskInfo => _taskInfo;
        
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly RoomDtoStorage _roomDtoStorage;
        private readonly TaskStorage _taskStorage;
        private readonly IRoomsSystem _roomsSystem;
        private TaskInfo _taskInfo;

        private const string _digTaskName = nameof(GetRoomDigTask);
        
        public DigAssignableTaskProcessor(UnitDtoStorage unitDtoStorage,
            IRoomsSystem roomsSystem,
            TaskStorage taskStorage,
            UnitTaskProcessorStorage unitTaskProcessorStorage,
            RoomDtoStorage roomDtoStorage)
        {
            _roomsSystem = roomsSystem;
            _taskStorage = taskStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _unitDtoStorage = unitDtoStorage;
            _roomDtoStorage = roomDtoStorage;
        }

        public bool CanExecute(string guid)
        {
            var unitTaskProcessor = _unitTaskProcessorStorage.Get(guid);

            /*var currentTask= unitTaskProcessor.GetCurrentTask();
            if (currentTask != null && _taskStorage.GetTaskInfo(currentTask.Guid).TaskName == _digTaskName)
            {
                return false;
            }*/
            
            var dto = _unitDtoStorage.Get(guid);
            if (dto.ModelID != "8")
            {
                return false;
            }

            var openedCount = _roomsSystem.Opened().Count();
            if (openedCount == _roomDtoStorage.Count)
            {
                return false;
            }

            var allTasks = _taskStorage.GetAllInfo();
            var digTasks = allTasks.Where(x => x.TaskName == _digTaskName);

            foreach (var taskInfo in digTasks)
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
            if (taskInfo != null && taskInfo.TaskName == _digTaskName || !_taskStorage.HasTask(taskInfo))
            {
                return;
            }
            var task = _taskStorage.Get(_taskInfo.TaskGuid);
            if (task != null)
                return;
            _unitTaskProcessorStorage.Get(guid).SetTask(task);
        }
    }
}