using System.Collections.Generic;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;
using Event = Spine.Event;

namespace BugsFarm.BuildingSystem
{
    public class GoldminingTask : BaseTask
    {
        private readonly IInstantiator _instantiator;
        private readonly ISoundSystem _soundSystem;
        private readonly ISimulationSystem _simulationSystem;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly AnimatorStorage _animatorStorage;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        private string[] Sounds { get; } = {"Mattock1", "Mattock2", "Lift"};
        private const string _resourceStatKey = "stat_capacity";
        private const string _efficiencyStatKey = "stat_efficiency";
        private const int _repeatMax = 7;

        // Unit
        private string _unitGuid;
        private ISpineAnimator _unitAnimator;

        // Goldmine
        private readonly string _buildingGuid;
        private ISpineAnimator _goldmineAnimator;
        private GoldmineSceneObject _view;
        private AudioModel _audioModel;
        private StatVital _capacityStat;
        private StatModifiable _efficiencyStat;

        // Task
        private readonly IPosSide _pointLever;
        private readonly IPosSide _pointRocks;
        private ITask _moveTask;
        private ITask _simulationTask;
        private int _repeatCounter;
        private int _simulationChunkSeconds;
        private const float _otherSimulations = 0.5f;

        public GoldminingTask(string buildingGuid,
                               IEnumerable<IPosSide> taskPoints,
                               IInstantiator instantiator,
                               ISoundSystem soundSystem,
                               ISimulationSystem simulationSystem,
                               SceneEntityStorage sceneEntityController,
                               AnimatorStorage animatorStorage,
                               AudioModelStorage audioModelStorage,
                               BuildingSceneObjectStorage viewStorage,
                               StatsCollectionStorage statsCollectionStorage)
        {
            _buildingGuid = buildingGuid;
            _instantiator = instantiator;
            _soundSystem = soundSystem;
            _simulationSystem = simulationSystem;
            _sceneEntityController = sceneEntityController;
            _animatorStorage = animatorStorage;
            _audioModelStorage = audioModelStorage;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
            var points = taskPoints.ToArray();
            _pointLever = points[0];
            _pointRocks = points[1];
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);

            _unitGuid = (string) args[0];
            var statCollection = _statsCollectionStorage.Get(_buildingGuid);
            var entity = _sceneEntityController.Get(_buildingGuid);

            _audioModel = _audioModelStorage.Get(entity.GetType().Name);
            _view = (GoldmineSceneObject) _viewStorage.Get(_buildingGuid);
            _capacityStat = statCollection.Get<StatVital>(_resourceStatKey);
            _efficiencyStat = statCollection.Get<StatModifiable>(_efficiencyStatKey);
            _goldmineAnimator = _animatorStorage.Get(_buildingGuid);
            _unitAnimator = _animatorStorage.Get(_unitGuid);

            _goldmineAnimator.OnAnimationEvent += OnGoldmneAnimationEvent;
            _goldmineAnimator.OnAnimationComplete += OnGoldmineAnimationComplete;
            _unitAnimator.OnAnimationComplete += OnUnitAnimationComplete;
            _simulationSystem.OnSimulationStart += OnSimulationStart;
            _simulationSystem.OnSimulationEnd += OnSimulationEnded;
            _simulationChunkSeconds = Mathf.RoundToInt(_unitAnimator.GetDuration(AnimKey.MineRock) + _otherSimulations);
            if (_simulationSystem.Simulation)
            {
                CreateSimulationTask();
            }
            else
            {
                OnLever();
            }
        }

        private ITask CreateMoveTask(IPosSide point)
        {
            return _instantiator.Instantiate<MoveToSceneItemTask>(new object[] {_buildingGuid, point});
        }

        private void RockProcess()
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            if (++_repeatCounter <= _repeatMax)
            {
                SetProgressPerStep();
                if (!CanMining() || !IsRunned || IsCompleted) return;
                _unitAnimator.SetAnim(AnimKey.MineRock);
                SetSound(Sounds[Random.Range(0, 1)]);
            }
            else
            {
                _view.SetActiveRock(false);
                _repeatCounter = 0;
                if (!CanMining())
                {
                    Completed();
                }
                else
                {
                    _moveTask = CreateMoveTask(_pointLever);
                    _moveTask.OnComplete += OnMoveToLeverComplete;
                    _moveTask.Execute(_unitGuid);
                }
            }
        }

        private void SetProgressPerStep()
        {
            if (!IsRunned || IsCompleted) return;
            _capacityStat.CurrentValue += _efficiencyStat.Value / _repeatMax;
            if (!CanMining())
            {
                Completed();
            }
        }

        private bool CanMining()
        {
            if (!IsRunned || IsCompleted) return false;
            return _capacityStat.CurrentValue < _capacityStat.Value;
        }

        private void SetSound(string audioKey)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            _soundSystem.Play(_view.transform.position,_audioModel.GetAudioClip(audioKey));
        }

        private void OnLever()
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            _view.SetActiveRock(false);
            _unitAnimator.SetAnim(AnimKey.MineLever);
            _goldmineAnimator.SetAnim(AnimKey.MineLever);
        }

        private void OnRock()
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            if (_moveTask != null || _goldmineAnimator.CurrentAnim != AnimKey.Idle || _repeatCounter > 0) return;

            _repeatCounter = 0;
            RockProcess();
        }

        private void OnMoveToRockTaskComplete(ITask task)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            _moveTask = null;
            _unitAnimator.SetAnim(AnimKey.Idle);
            OnRock();
        }

        private void OnMoveToLeverComplete(ITask task)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            _moveTask = null;
            OnLever();
        }

        private void OnUnitAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            switch (animKey)
            {
                case AnimKey.MineLever:
                    _moveTask = CreateMoveTask(_pointRocks);
                    _moveTask.OnComplete += OnMoveToRockTaskComplete;
                    _moveTask.Execute(_unitGuid);
                    break;
                case AnimKey.MineRock:
                    RockProcess();
                    break;
            }
        }

        private void OnGoldmineAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            if (animKey == AnimKey.MineLever)
            {
                _view.SetActiveRock(true);
                _goldmineAnimator.SetAnim(AnimKey.Idle);
                OnRock();
            }
        }

        private void OnGoldmneAnimationEvent(AnimKey animKey, Event spineEvent)
        {
            if (!IsRunned || IsCompleted || _simulationSystem.Simulation) return;
            var eventName = spineEvent?.Data.Name;
            if (string.IsNullOrEmpty(eventName) || eventName != "Sound")
            {
                return;
            }

            SetSound(Sounds[2]);
        }

        private void OnSimulationEnded()
        {
            if (!IsRunned || IsCompleted) return;
            _simulationTask?.Interrupt();
            _simulationTask = null;
            if (CanMining())
            {
                _repeatCounter = 0;
                _moveTask = CreateMoveTask(_pointLever);
                _moveTask.OnComplete += OnMoveToLeverComplete;
                _moveTask.Execute(_unitGuid);
            }
            else
            {
                Completed();
            }
        }

        private void OnSimulationStart()
        {
            CreateSimulationTask();
        }

        private void CreateSimulationTask()
        {
            if (!IsRunned || IsCompleted || _simulationTask != null || !_simulationSystem.Simulation) return;
            _moveTask?.Interrupt();
            _moveTask = null;
            _goldmineAnimator.SetAnim(AnimKey.Idle);
            _unitAnimator.SetAnim(AnimKey.Idle);

            var simulationTask = _instantiator.Instantiate<SimulatedTimerTask>();
            simulationTask.SetChunkAction(SetProgressPerStep, _simulationChunkSeconds);
            simulationTask.OnComplete += _ =>
            {
                _simulationTask = null;
                if (CanMining())
                {
                    CreateSimulationTask();
                }
                else
                {
                    Completed();
                }
                
            };
            _simulationTask = simulationTask;
            _simulationTask.Execute(float.MaxValue);
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _moveTask?.Interrupt();
                _moveTask = null;
                _simulationTask?.Interrupt();
                _simulationTask = null;
                
                _view.SetActiveRock(false);
                _goldmineAnimator.OnAnimationEvent -= OnGoldmneAnimationEvent;
                _goldmineAnimator.OnAnimationComplete -= OnGoldmineAnimationComplete;
                _unitAnimator.OnAnimationComplete -= OnUnitAnimationComplete;
                _simulationSystem.OnSimulationStart -= OnSimulationStart;
                _simulationSystem.OnSimulationEnd -= OnSimulationEnded;
            }

            _unitAnimator = null;
            _goldmineAnimator = null;
            _view = null;
            _capacityStat = null;
            _efficiencyStat = null;
            _audioModel = null;
        }
    }
}