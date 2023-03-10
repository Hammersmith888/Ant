using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulatingSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.UI
{
    public class TrainAssignableTaskProcessor : IUnitAssignableTaskProcessor
    {
        public TaskInfo TaskInfo => _taskInfo;
        
        private readonly SimulatingTrainingModelStorage _simulatingTrainingModelStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly TaskStorage _taskStorage;
        private BuildingDto _trainingBuildingDto;

        private const string _maxUnitsStatKey = "stat_maxUnits";

        private Dictionary<string, string> _trainingBuildingsNames;
        private TaskInfo _taskInfo;

        public TrainAssignableTaskProcessor(UnitDtoStorage unitDtoStorage,
            BuildingDtoStorage buildingDtoStorage,
            UnitMoverStorage unitMoverStorage,
            UnitTaskProcessorStorage unitTaskProcessorStorage,
            TaskStorage taskStorage,
            StatsCollectionStorage statsCollectionStorage,
            SimulatingTrainingModelStorage simulatingTrainingModelStorage)
        {
            _taskStorage = taskStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _unitDtoStorage = unitDtoStorage;
            _unitMoverStorage = unitMoverStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _simulatingTrainingModelStorage = simulatingTrainingModelStorage;

            _trainingBuildingsNames = new Dictionary<string, string>()
            {
                {"58", nameof(TrainingSwordmanBootstrapTask)},
                {"52", nameof(TrainingPikemanBootstrapTask)},
                {"38", nameof(TrainingArcherBootstrapTask)},
                {"46", nameof(TrainingButterflyBootstrapTask)},
                {"51", nameof(TrainingHeavySquadBootstrapTask)}
            };
        }

        public bool CanExecute(string guid)
        {
            var dto = _unitDtoStorage.Get(guid);

            if (!_simulatingTrainingModelStorage.HasEntity(dto.ModelID))
            {
                return false;
            }
            
            var allowedTrainBuildings = _simulatingTrainingModelStorage.Get(dto.ModelID).BuildingsModelID.ToList();
            var unitTaskProcessor = _unitTaskProcessorStorage.Get(guid);

          /*  foreach (var trainBuilding in allowedTrainBuildings)
            {
                var currentTask = unitTaskProcessor.GetCurrentTask();
                if (currentTask != null && _taskStorage.GetTaskInfo(currentTask.Guid).TaskName == _trainingBuildingsNames[trainBuilding])
                {
                    return false;
                }
            }*/
            
            var trainingBuildings = _buildingDtoStorage.Get().Where(x => allowedTrainBuildings.Contains(x.ModelID));

            if (!trainingBuildings.Any())
            {
                return false;
            }

            var _trainingBuildingDto = trainingBuildings.First();
            
            var mover = _unitMoverStorage.Get(guid);
            var allTasks = _taskStorage.GetAllInfo().Where(x => x.TaskName == _trainingBuildingsNames[_trainingBuildingDto.ModelID]);
            
            
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

            if (taskInfo != null)
            {
                foreach (var trainingName in _trainingBuildingsNames.Values)
                {
                    if (taskInfo.TaskName == trainingName)
                    {
                        return;
                    }
                }
            }

            var task = _taskStorage.Get(_taskInfo.TaskGuid);
            
            if(task == null)
                return;
            
            _unitTaskProcessorStorage.Get(guid).SetTask(task);
        }
    }
}