using System;
using System.Collections.Generic;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseTraining : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }
        protected abstract Type TrainTaskType { get; }

        private BuildingSceneObjectStorage _viewStorage;
        private StatsCollectionStorage _statsCollectionStorage;
        private BuildingDtoStorage _dtoStorage;
        private IBuildingBuildSystem _buildingBuildSystem;
        private IInstantiator _instantiator;
        private TaskStorage _taskStorage;
        private const string _maxUnitsStatKey = "stat_maxUnits";
        
        private StatsCollection _statsCollection;
        private PointsController _pointsController;
        private List<ITask> _trainingTasks;
        private bool _finalized;
        
        [Inject]
        private void Inject(string guid,
                            IInstantiator instantiator,
                            TaskStorage taskStorage,
                            BuildingSceneObjectStorage viewStorage,
                            BuildingDtoStorage dtoStorage,
                            StatsCollectionStorage statsCollectionStorage,
                            IBuildingBuildSystem buildingBuildSystem)
        {
            Id = guid;
            _instantiator = instantiator;
            _taskStorage = taskStorage;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _buildingBuildSystem = buildingBuildSystem;
            _dtoStorage = dtoStorage;
        }

        public void Initialize()
        {
            _trainingTasks = new List<ITask>();
            
            _statsCollection = _statsCollectionStorage.Get(Id);
            var view = _viewStorage.Get(Id);
            var taskPoints = Tools.Shuffle_FisherYates(view.GetComponent<TasksPoints>().Points);
            
            _pointsController = new PointsController();
            _pointsController.Initialize((int)_statsCollection.GetValue(_maxUnitsStatKey), taskPoints);

            _buildingBuildSystem.Registration(Id);
            _buildingBuildSystem.OnStarted   += OnBuildingStarted;
            _buildingBuildSystem.OnCompleted += OnBuildingCompleted;
            
            if (_buildingBuildSystem.CanBuild(Id))
            {
                _buildingBuildSystem.Start(Id);
            }
            else
            {
                Production();
            }
        }
        
        public void Dispose()
        {
            if(_finalized) return;
            _buildingBuildSystem.OnStarted -= OnBuildingStarted;
            _buildingBuildSystem.OnCompleted -= OnBuildingCompleted;
            _buildingBuildSystem.UnRegistration(Id);
            FreeTrainingsTasks();
            _pointsController.Dispose();
            _finalized = true;
        }
        
        private void Production()
        {
            if(_finalized) return;
            var dto = _dtoStorage.Get(Id);
            while (_pointsController.HasPoint())
            {
                var taskPoint = _pointsController.GetPoint();
                var args = new object[] {Id, taskPoint};
                var trainTask = (ITask)_instantiator.Instantiate(TrainTaskType, args);

                trainTask.OnInterrupt += OnTraininEnd;
                trainTask.OnComplete  += OnTraininEnd;
                _trainingTasks.Add(trainTask);
                _taskStorage.DeclareTask(Id, dto.ModelID, trainTask.GetName(), trainTask, false);
            }
        }
        
        private void FreeTrainingsTasks()
        {
            if(_finalized) return;
            var tasksCopy = _trainingTasks.ToArray();
            foreach (var task in tasksCopy)
            {
                if(task.IsNullOrDefault()) continue;
                
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
            if(_finalized)return;
            
            if (_trainingTasks.Contains(task))
            {
                _pointsController.FreePoint();
                _trainingTasks.Remove(task);
                _taskStorage.Remove(task.Guid);
            }   
            if(!_buildingBuildSystem.CanBuild(Id))
            {
                Production();
            }
        }

        private void OnBuildingStarted(string guid)
        {
            if (guid != Id || _finalized)
            {
                return;
            }
            FreeTrainingsTasks();
        }
        
        private void OnBuildingCompleted(string guid)
        {
            if (guid != Id || _finalized)
            {
                return;
            }
            if (!_buildingBuildSystem.CanBuild(Id))
            {
                Production();
            }
        }
    }
}