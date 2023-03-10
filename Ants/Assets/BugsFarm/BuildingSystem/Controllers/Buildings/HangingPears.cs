using System.Collections.Generic;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class HangingPears : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }

        private BuildingSceneObjectStorage _viewStorage;
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingDtoStorage _dtoStorage;
        private IInstantiator _instantiator;
        private TaskStorage _taskStorage;

        private const string _maxUnitsStatKey = "stat_maxUnits";

        private PointsController _pointsController;
        private List<ITask> _trainingTasks;
        private bool _finalized;

        [Inject]
        private void Inject(string guid,
                            IInstantiator instantiator,
                            TaskStorage taskStorage,
                            BuildingSceneObjectStorage viewStorage,
                            BuildingDtoStorage dtoStorage,
                            StatsCollectionStorage statCollectionStorage)
        {
            Id = guid;
            _instantiator = instantiator;
            _taskStorage = taskStorage;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statCollectionStorage;
            _dtoStorage = dtoStorage;
        }

        public void Initialize()
        {
            _trainingTasks = new List<ITask>();

            var statCollection = _statsCollectionStorage.Get(Id);
            var view = _viewStorage.Get(Id);
            var taskPoints = Tools.Shuffle_FisherYates(view.GetComponent<TasksPoints>().Points);
            var maxUnits = (int) statCollection.GetValue(_maxUnitsStatKey);


            _pointsController = new PointsController();
            _pointsController.Initialize(maxUnits, taskPoints);

            Production();
        }

        public void Dispose()
        {
            if (_finalized) return;
            _finalized = true;
            _pointsController.Dispose();
            FreeTrainingsTasks();
        }

        private void Production()
        {
            if (_finalized) return;

            var dto = _dtoStorage.Get(Id);
            while (_pointsController.HasPoint())
            {
                var taskPoint = _pointsController.GetPoint();
                var args = new object[] {Id, taskPoint};
                var trainTask = _instantiator.Instantiate<TrainingButterflyBootstrapTask>(args);
                trainTask.OnInterrupt += OnTraininEnd;
                trainTask.OnComplete += OnTraininEnd;
                _trainingTasks.Add(trainTask);

                _taskStorage.DeclareTask(Id, dto.ModelID, trainTask.GetName(), trainTask, false);
            }
        }

        private void FreeTrainingsTasks()
        {
            var tasksCopy = _trainingTasks.ToArray();
            foreach (var task in tasksCopy)
            {
                if (task == null)
                {
                    continue;
                }

                if (_taskStorage.HasTask(task.Guid))
                {
                    _taskStorage.Remove(task.Guid);
                }

                task.Interrupt();
            }

            _trainingTasks.Clear();
        }

        private void OnTraininEnd(ITask task)
        {
            if (_finalized) return;
            if (_trainingTasks.Contains(task))
            {
                _trainingTasks.Remove(task);
                _pointsController.FreePoint();
            }

            Production();
        }
    }
}