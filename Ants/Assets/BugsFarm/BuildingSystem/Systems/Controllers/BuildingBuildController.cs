using System;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SimpleLocalization;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingBuildController : IInitializable, ITickable, IDisposable
    {
        public ITask BuildingTask { get; }
        public string BuildingGuid { get; }

        private readonly IMonoPool _monoPool;
        private readonly IInstantiator _instantiator;
        private readonly ISimulationSystem _simulationSystem;
        private readonly UIWorldRoot _uiWorldRoot;
        private readonly ITickableManager _tickableManager;
        private readonly StatVital _resourceStat;
        private readonly Transform _progressPoint;
        private const string _progressBarPoint = "UIProgressPoint";
        private const string _buildingTextKey = "BuildingInner_Building";
        private string _buildingText;
        private UIProgress _progress;
        private BuildingInfo _buildingInfo;
        private StateInfo _stateInfoTemp;
        private IStateInfo _stateInfo;
        private bool _finalized;
        private bool _initialized;
        public BuildingBuildController(string buildingGuid, 
                                      ITask buildingTask,
                                      StatVital resourceStat,
                                      IMonoPool monoPool,
                                      IInstantiator instantiator,
                                      ISimulationSystem simulationSystem,
                                      UIWorldRoot uiWorldRoot,
                                      ITickableManager tickableManager,
                                      BuildingSceneObjectStorage viewStorage)
        {
            BuildingGuid = buildingGuid;
            BuildingTask = buildingTask;
            _monoPool = monoPool;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _uiWorldRoot = uiWorldRoot;
            _tickableManager = tickableManager;
            _resourceStat = resourceStat;
            _progressPoint = viewStorage.Get(buildingGuid).transform.Find(_progressBarPoint);
        }

        public void Initialize()
        {
            if (_finalized || _initialized)
            {
                return;
            }
            
            _initialized = true;
            
            if (!_progress)
            {
                _progress = _monoPool.Spawn<UIProgress>(_uiWorldRoot.transform);
                _progress.ChangePosition(_progressPoint.position);
            }
            
            _buildingInfo = _instantiator.Instantiate<BuildingInfo>(new object[]{BuildingGuid});
            _buildingText = LocalizationManager.Localize(_buildingTextKey);
            _tickableManager.Add(this);
        }

        public void Tick()
        {
            if (_finalized || !_initialized || _simulationSystem.Simulation)
            {
                return;
            }

            if (!_progressPoint || !_progress)
            {
                return;
            }

            var currentTime = Format.MinutesToSeconds(_resourceStat.CurrentValue);
            var formatTime = Format.Age(currentTime);
            var progress = 1f - (_resourceStat.CurrentValue / _resourceStat.Value);
            _progress.ChangeText(formatTime);
            _progress.ChangeProgress(progress);
            _progress.ChangePosition(_progressPoint.position);
            _buildingInfo.SetInfo(progress, formatTime, _buildingText);
        }

        public void Dispose()
        {
            if(_finalized || !_initialized) return;
            _finalized = true;
            _initialized = false;
            
            if (_progress)
            {
                _monoPool.Despawn(_progress);
                _progress = null;
            }
            _buildingInfo.Dispose();
            _buildingInfo = null;
            _tickableManager.Remove(this);
        }
    }
}