using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GardenCareTask : BaseTask
    {
        private readonly ISoundSystem _soundSystem;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IInstantiator _instantiator;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly AnimatorStorage _animatorStorage;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private string[] Sounds { get; } = {"Care1", "Care2"};
        private AnimKey[] Anims { get; } = {AnimKey.Garden1, AnimKey.Garden2};
        private readonly Vector2Int _cyclesRange = new Vector2Int(2, 6);
        private readonly Vector2Int _taskRange = new Vector2Int(2, 4 + 1);
        
        private readonly IPosSide[] _taskPoints;
        private readonly string _buildingGuid;
        
        private string _unitGuid;
        private BuildingSceneObject _sceneObject;
        private AudioModel _audioModel;
        private ISpineAnimator _animator;
        private IDisposable _moveEvent;
        private ITask _moveTask;
        private AnimKey _currAnimKey;
        private int _taskCount;
        private int _cycle;

        public GardenCareTask(string buildingGuid,
                              IEnumerable<IPosSide> taskPoints,
                              ISoundSystem soundSystem,
                              ISimulationSystem simulationSystem,
                              IInstantiator instantiator,
                              SceneEntityStorage sceneEntityController,
                              AnimatorStorage animatorStorage,
                              AudioModelStorage audioModelStorage,
                              BuildingSceneObjectStorage buildingSceneObjectStorage)
        {
            _buildingGuid = buildingGuid;
            _soundSystem = soundSystem;
            _simulationSystem = simulationSystem;
            _instantiator = instantiator;
            _sceneEntityController = sceneEntityController;
            _animatorStorage = animatorStorage;
            _audioModelStorage = audioModelStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _taskPoints = taskPoints.ToArray();
            _taskCount = Tools.RandomRange(_taskRange);
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            _unitGuid = (string) args[0];
            var entity = _sceneEntityController.Get(_buildingGuid);
            _sceneObject = _buildingSceneObjectStorage.Get(_buildingGuid);
            _audioModel = _audioModelStorage.Get(entity.GetType().Name);
            _animator = _animatorStorage.Get(_unitGuid);
            _animator.OnAnimationComplete += OnAnimationComplete;
            _moveEvent = MessageBroker.Default.Receive<PlaceBuildingProtocol>().Subscribe(OnSceneItemMoved);
            CreateMoveTask();
        }

        private void OnSceneItemMoved(PlaceBuildingProtocol protocol)
        {
            if (!IsRunned || IsCompleted) return;
            if (_buildingGuid != protocol.Guid) return;
            _animator.SetAnim(AnimKey.Idle);
            CreateMoveTask();
        }

        private void CreateMoveTask()
        {
            if (!IsRunned || IsCompleted) return;
            
            _moveTask?.Interrupt();
            _moveTask = null;

            var randomPoint = Tools.RandomItem(_taskPoints);
            _moveTask = _instantiator.Instantiate<MoveToSceneItemTask>(new object[] {_buildingGuid, randomPoint});
            _moveTask.OnComplete  += OnDestinationComplete;
            _moveTask.OnInterrupt += OnDestinationComplete;
            _moveTask.Execute(_unitGuid);
        }

        private void OnDestinationComplete(ITask task)
        {
            if (!IsRunned || IsCompleted) return;
            _moveTask = null;
            _cycle = Tools.RandomRange(_cyclesRange);
            _currAnimKey = Tools.RandomItem(Anims);
            OnGardenCare();
        }

        private void OnGardenCare()
        {
            if (!IsRunned || IsCompleted || _moveTask != null) return;
            if (--_cycle <= 0)
            {
                if (--_taskCount <= 0)
                {
                    Completed();
                    return;
                }
                CreateMoveTask();
            }
            else
            {
                _animator.SetAnim(_currAnimKey);

                if (_simulationSystem.Simulation) return;
                var audioKey = Tools.RandomItem(Sounds);
                var audioPath = _audioModel.GetAudioClip(audioKey);
                _soundSystem.Play(_sceneObject.transform.position, audioPath);
            }
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned || IsCompleted || animKey != _currAnimKey || _moveTask != null) return;
            OnGardenCare();
        }
        
        public override Vector2[] GetPositions()
        {
            return _taskPoints.ToPositions();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
            }

            _moveEvent?.Dispose();
            _moveEvent = null;
            _moveTask?.Interrupt();
            _moveTask = null;
            _audioModel = null;
            _animator = null;
            base.OnDisposed();
        }
    }
}