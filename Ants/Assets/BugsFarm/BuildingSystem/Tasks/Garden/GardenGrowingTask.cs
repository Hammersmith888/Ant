using System.Collections.Generic;
using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GardenGrowingTask : BaseTask
    {

        private readonly TaskStorage _taskStorage;
        private readonly IInstantiator _instantiator;
        private readonly AnimatorStorage _animatorStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly BuildingDtoStorage _buildingDtoStorge;
        private readonly BuildingStageModelStorage _stageModelsStorage;
        private readonly IEnumerable<IPosSide> _taskpoints;
        private readonly string _buildingGuid;
        private const string _growTimeStatKey = "stat_growTime";

        private ISpineAnimator _gardenAnimator;
        private StatVital _growTimeStat;
        private ITask _careTask;
        private ITask _taskTimer;

        public GardenGrowingTask(string buildingGuid,
                                 IEnumerable<IPosSide> taskpoints,
                                 TaskStorage taskStorage,
                                 IInstantiator instantiator,
                                 AnimatorStorage animatorStorage,
                                 StatsCollectionStorage statsCollectionStorage,
                                 BuildingDtoStorage buildingDtoStorge,
                                 BuildingStageModelStorage stageModelsStorage)
        {
            _buildingGuid = buildingGuid;
            _taskpoints = taskpoints;
            _taskStorage = taskStorage;
            _instantiator = instantiator;
            _animatorStorage = animatorStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingDtoStorge = buildingDtoStorge;
            _stageModelsStorage = stageModelsStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            var dto = _buildingDtoStorge.Get(_buildingGuid);
            var buildingStageModel = _stageModelsStorage.Get(dto.ModelID);
            var statCollection = _statsCollectionStorage.Get(_buildingGuid);
            _growTimeStat = statCollection.Get<StatVital>(_growTimeStatKey);
            _gardenAnimator = _animatorStorage.Get(_buildingGuid);
            
            var updateChunkSeconds = (int) Format.MinutesToSeconds(_growTimeStat.Value / buildingStageModel.Count);
            var taskTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            taskTimer.SetChunkAction(UpdateGrowing, updateChunkSeconds);
            taskTimer.OnComplete += _ => Completed();
            _taskTimer = taskTimer;
            _taskTimer.Execute(_buildingGuid, _growTimeStatKey);
            UpdateGrowing();
        }

        private void UpdateGrowing()
        {
            if (!IsRunned || IsCompleted || Finalized) return;
            UpdateStage();

            if (_careTask == null)
            {
                var args = new object[] {_buildingGuid, _taskpoints};
                _careTask = _instantiator.Instantiate<GardenCareTask>(args);

                if (!IsRunned || IsCompleted || Finalized)
                {
                    RemoveCareTask();
                    return;
                }
                
                var dto = _buildingDtoStorge.Get(_buildingGuid);
                _careTask.OnComplete  += OnCareTaskEnd;
                _careTask.OnInterrupt += OnCareTaskEnd;
                _taskStorage.DeclareTask(_buildingGuid, dto.ModelID, _careTask.GetName(), _careTask);
            }
        }

        private void RemoveCareTask()
        {
            if (_careTask != null && _taskStorage.HasTask(_careTask.Guid))
            {
                _taskStorage.Remove(_careTask.Guid);
            }
            _careTask?.Interrupt();
            _careTask = null;
        }

        private void UpdateStage()
        {
            if (!IsRunned || IsCompleted || Finalized) return;

            var protocol =
                new SetStageBuildingProtocol(_buildingGuid, _growTimeStat.CurrentValue, _growTimeStat.Value);
            _instantiator.Instantiate<SetStageBuildingCommand>().Execute(protocol);
            _gardenAnimator.SetAnim(AnimKey.Idle);
        }

        private void OnCareTaskEnd(ITask task)
        {
            if (!IsRunned || IsCompleted || Finalized) return;
            RemoveCareTask();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                UpdateStage();
            }

            RemoveCareTask();
            _taskTimer?.Interrupt();
            _taskTimer = null;
            _gardenAnimator = null;
            _growTimeStat = null;
            _careTask = null;
            Finalized = true;
        }
    }
}