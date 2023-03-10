using System;
using System.Collections.Generic;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingBuildSystem : IBuildingBuildSystem
    {
        public event Action<string> OnCompleted;
        public event Action<string> OnStarted;
        
        private readonly IInstantiator _instantiator;
        private readonly TaskStorage _taskStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingDtoStorage _dtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private const string _buildingTimeStatKey = "stat_buildingTime";
        private const float _buildingAlpha = 0.5f;
        private const float _buildingAlphaNormal = 1;

        /// <summary>
        /// arg1 = идентификатор постройки , arg2 = стат времени стройки
        /// </summary>
        private readonly Dictionary<string, StatVital> _storage;
        
        /// <summary>
        /// arg1 = идентификатор постройки, arg2 = связанный контроллер
        /// </summary>
        private readonly Dictionary<string, BuildingBuildController> _buildings;

        public BuildingBuildSystem(IInstantiator instantiator,
                                   TaskStorage taskStorage,
                                   BuildingSceneObjectStorage viewStorage,
                                   BuildingDtoStorage dtoStorage,
                                   StatsCollectionStorage statsCollectionStorage)
        {
            _instantiator = instantiator;
            _taskStorage = taskStorage;
            _viewStorage = viewStorage;
            _dtoStorage = dtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _storage = new Dictionary<string, StatVital>();
            _buildings = new Dictionary<string, BuildingBuildController>();
        }

        public void Registration(string guid)
        {
            if (HasEntity(guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Registration)} :: Building with [ Guid : {guid}], alredy exist.");
            }

            if (!_statsCollectionStorage.HasEntity(guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Registration)} :: Building with [ Guid : {guid}], {nameof(StatsCollection)} does not exist.");
            }

            var statsCollection = _statsCollectionStorage.Get(guid);
            _storage.Add(guid, statsCollection.Get<StatVital>(_buildingTimeStatKey));
        }

        public void UnRegistration(string guid)
        {
            if (!HasEntity(guid))
            {
                return;
            }

            var stat = _storage[guid];
            _storage.Remove(guid);
            if (IsBuilding(guid))
            {
                var controller = _buildings[guid];
                if(controller != null)
                {
                    if (_taskStorage.HasTask(controller.BuildingTask.Guid))
                    {
                        _taskStorage.Remove(controller.BuildingTask.Guid);
                    }
                    controller.BuildingTask?.Interrupt();
                    SetAlpha(guid, _buildingAlphaNormal);
                    controller.Dispose();
                }
                _buildings.Remove(guid);
            }
        }

        public void Start(string buildingGuid)
        {
            if (!HasEntity(buildingGuid))
            {
                Debug.LogError($"{this} : {nameof(Start)} :: Building with [ Guid : {buildingGuid}], does not exist.");
                return;
            }
            
            if (!_viewStorage.HasEntity(buildingGuid))
            {
                Debug.LogError($"{this} : {nameof(Registration)} :: Building with [ Guid : {buildingGuid}], View does not exist");
                return;
            }
            
            if (!CanBuild(buildingGuid))
            {
                Debug.Log($"{this} : {nameof(Start)} :: Building with [Guid : {buildingGuid}], Task time must be greater than zero.");
                return;
            }
            
            var view = _viewStorage.Get(buildingGuid);
            if (!view.TryGetComponent(out BuildingPoints buildingPoints))
            {
                Debug.LogError($"{this} : {nameof(Start)} :: Building with [Guid : {buildingGuid}], {nameof(BuildingPoints)} does not exist");
                return;
            }
            
            var extraArgs = new object[] {buildingGuid, buildingPoints.Points};
            var buildTask = _instantiator.Instantiate<BuildingBootstrapTask>(extraArgs);
            buildTask.OnInterrupt += _ => OnInterrupt(buildingGuid);
            buildTask.OnComplete  += _ => OnTaskComplete(buildingGuid);
            var argsController = new object[] {buildingGuid, buildTask, _storage[buildingGuid]};
            var controller = _instantiator.Instantiate<BuildingBuildController>(argsController);
            var dto = _dtoStorage.Get(buildingGuid);
            controller.Initialize();
            _buildings.Add(buildingGuid,controller);
            SetAlpha(buildingGuid, _buildingAlpha);
            OnStarted?.Invoke(buildingGuid);
            _taskStorage.DeclareTask(buildingGuid, dto.ModelID, buildTask.GetName(), buildTask);
        }

        public bool HasEntity(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public bool IsBuilding(string guid)
        {
            return _buildings.ContainsKey(guid);
        }

        public bool CanBuild(string guid)
        {
            return !IsBuilding(guid) && HasEntity(guid) && _storage[guid].CurrentValue > 0;
        }
        public void ForceComplete(string guid)
        {
            if (!IsBuilding(guid) || !HasEntity(guid))
            {
                return;
            }

            _storage[guid].CurrentValue = 0;
            var controller = _buildings[guid];
            if(controller?.BuildingTask != null)
            {
                if (_taskStorage.HasTask(controller.BuildingTask.Guid))
                {
                    _taskStorage.Remove(controller.BuildingTask.Guid);
                }
                controller.BuildingTask.Interrupt();
            }
            else
            {
                _buildings.Remove(guid);
                controller?.Dispose();
            }
            SetAlpha(guid, _buildingAlphaNormal);
            OnCompleted?.Invoke(guid);
        }

        public bool GetTime(string guid, out float current, out float maximum)
        {
            current = maximum = 0;
            if (!HasEntity(guid))
            {
                return false;
            }
            if (!IsBuilding(guid))
            {
                return false;
            }

            var stat = _storage[guid];
            current = stat.CurrentValue;
            maximum = stat.Value;
            return true;
        }

        public void ResetBuildingTime(string guid)
        {
            if (!HasEntity(guid))
            {
                return;
            }
            
            _storage[guid].SetMax();
        }

        public void AddBuildingTime(string guid, float timeToAdd)
        {
            if (!HasEntity(guid))
            {
                return;
            }
            _storage[guid].CurrentValue += timeToAdd;
        }
        
        private void SetAlpha(string guid, float alpha)
        {
            if (_viewStorage.HasEntity(guid))
            {
                var view = _viewStorage.Get(guid);
                view.SetAlpha(alpha);
            }
        }

        private void OnInterrupt(string guid)
        {
            if (IsBuilding(guid))
            {
                var controller = _buildings[guid];
                _buildings.Remove(guid);
                controller.Dispose();
            }
            
            if (CanBuild(guid))
            {
                Start(guid);
            }
        }
        
        private void OnTaskComplete(string buildingGuid)
        {
            if (!IsBuilding(buildingGuid)) return;
            
            var controller = _buildings[buildingGuid];
            _buildings.Remove(buildingGuid);
            controller?.Dispose();
            
            SetAlpha(buildingGuid, _buildingAlphaNormal);
            OnCompleted?.Invoke(buildingGuid);
        }
    }
}