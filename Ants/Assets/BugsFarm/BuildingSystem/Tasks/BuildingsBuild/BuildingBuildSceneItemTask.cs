using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingBuildSceneItemTask : BaseTask
    {
        private readonly ISoundSystem _soundSystem;
        private readonly IInstantiator _instantiator;
        private readonly ISimulationSystem _simulationSystem;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        private readonly string _buildingGuid;
        private readonly IPosSide[] _buildPoints;
        private const string _buildingTimeStatKey = "stat_buildingTime";

        private AnimKey[] Anims { get; } = {AnimKey.Build1, AnimKey.Build2, AnimKey.Build3};
        private string[] Sounds { get; } = {"Hammer", "Hammer", "Saw"};
        private Vector2Int RepeatRange { get; } = new Vector2Int(4, 9);

        private object[] _taskArgs;
        private ITask _moveTask;
        private TimerFromStatKeyTask _buildTimer;
        private IDisposable _moveBuildingEvent;
        private ISpineAnimator _animator;
        private AudioModel _audioModel;
        private int _repeatCount;
        private string _currentSound;
        private AnimKey _currentAnim;
        private BuildingSceneObject _sceneObject;
        private StatVital _buildingStat;

        protected BuildingBuildSceneItemTask(string buildingGuid,
                                             IEnumerable<IPosSide> buildPoints,
                                             ISoundSystem soundSystem,
                                             IInstantiator instantiator,
                                             ISimulationSystem simulationSystem,
                                             SceneEntityStorage sceneEntityController,
                                             AudioModelStorage audioModelStorage,
                                             AnimatorStorage animatorStorage,
                                             BuildingSceneObjectStorage buildingSceneObjectStorage,
                                             StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _buildingGuid = buildingGuid;
            _soundSystem = soundSystem;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _sceneEntityController = sceneEntityController;
            _audioModelStorage = audioModelStorage;
            _animatorStorage = animatorStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _buildPoints = buildPoints.ToArray();
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(_taskArgs = args);
            var unitGuid = (string) args[0];
            var unit = _sceneEntityController.Get(unitGuid);

            _sceneObject = _buildingSceneObjectStorage.Get(_buildingGuid);
            _audioModel = _audioModelStorage.Get(unit.GetType().Name);
            _animator = _animatorStorage.Get(unitGuid);
            _animator.OnAnimationComplete += OnAnimationComplete;
            _simulationSystem.OnSimulationStart += OnSimulationStart;
            _simulationSystem.OnSimulationEnd += OnSimulationEnded;
            _moveBuildingEvent = MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(OnSceneItemMoved);
            if (!_simulationSystem.Simulation)
            {
                SetupBuild();
                InitBuildTimer();
                Build();
            }
            else
            {
                OnSimulationStart();
            }
        }

        private void InitBuildTimer()
        {
            if (!IsRunned || IsCompleted || _buildTimer != null || _moveTask != null) return;

            _buildTimer = _instantiator.Instantiate<TimerFromStatKeyTask>(new object[]{TimeType.Minutes});
            _buildTimer.SetUpdateAction(OnStatUpdated);
            _buildTimer.OnComplete += _ => Completed();
            _buildTimer.Execute(_buildingGuid, _buildingTimeStatKey);
            _buildingStat = _statsCollectionStorage.Get(_buildingGuid).Get<StatVital>(_buildingTimeStatKey);
        }

        private void OnStatUpdated(float timeLeft)
        {
            _buildingStat.CurrentValue = timeLeft;
        }

        private void InitMoveTask()
        {
            if (!IsRunned || IsCompleted) return;
            _moveTask?.Interrupt();
            _moveTask = _instantiator.Instantiate<MoveToSceneItemTask>(new object[]
            {
                _buildingGuid, Tools.RandomItem(_buildPoints)
            });
            _moveTask.OnComplete += _ => OnDestinationComplete();
            _moveTask.Execute(_taskArgs);
        }

        private void GoBuild()
        {
            if (!IsRunned || IsCompleted) return;
            _buildTimer?.Interrupt();
            _buildTimer = null;
            InitMoveTask();
        }

        private void Build()
        {
            if (!IsRunned || IsCompleted || _moveTask != null) return;

            if (!SetupBuild())
            {
                InitBuildTimer();
                _repeatCount--;
                _soundSystem.Play(_sceneObject.transform.position,_audioModel.GetAudioClip(_currentSound));
                _animator.SetAnim(_currentAnim);
            }
            else
            {
                InitMoveTask();
            }
        }

        private bool SetupBuild()
        {
            if (_repeatCount > 0) return false;

            _repeatCount = Tools.RandomRange(RepeatRange);
            _currentAnim = Tools.RandomItem(out var index, Anims);
            _currentSound = Sounds[index];
            return true;
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if(_simulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted || _currentAnim != animKey || _moveTask != null) return;
            Build();
        }

        private void OnDestinationComplete()
        {
            if(_simulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted || _moveTask == null) return;
            _moveTask?.Interrupt();
            _moveTask = null;
            Build();
        }

        private void OnSceneItemMoved(PlaceBuildingProtocol protocol)
        {
            if (_buildingGuid != protocol.Guid || !IsRunned || IsCompleted) return;
            GoBuild();
        }
        
        protected override void OnDisposed()
        {
            _buildTimer?.Interrupt();
            _buildTimer = null;

            _moveTask?.Interrupt();
            _moveTask = null;

            _moveBuildingEvent?.Dispose();
            _moveBuildingEvent = null;

            if (IsExecuted)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
                _simulationSystem.OnSimulationStart -= OnSimulationStart;
                _simulationSystem.OnSimulationEnd -= OnSimulationEnded;
            }

            _animator = null;
            _taskArgs = null;
            _audioModel = null;
        }

        private void OnSimulationEnded()
        {
            if (!IsRunned || IsCompleted || _moveTask != null) return;
            SetupBuild();
            Build();
        }

        private void OnSimulationStart()
        {
            if (!IsRunned || IsCompleted) return;
            _moveTask?.Interrupt();
            _moveTask = null;
            InitBuildTimer();
        }
    }
}