using BugsFarm.AnimationsSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;
using Event = Spine.Event;

namespace BugsFarm.BuildingSystem
{
    public class TrainingArcherTask : TrainingTask
    {
        protected override float DelaySecondsBetwenTrain => 0.5f;
        private UnitSceneObjectStorage _unitViewStorage;
        private BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private IMonoPool _monoPool;

        protected override AnimKey[] Anims { get; } = {AnimKey.Attack, AnimKey.Attack2};
        protected override string[] Sounds { get; } = {"Attack_1", "Attack_2"};
        private const string _projectilePointName = "ProjectilePoint";
        private const string _projectileTargetName = "ProjectileTarget";

        private Transform _projectilePoint;
        private Transform _projectileTarget;

        [Inject]
        private void Inject(UnitSceneObjectStorage unitViewStorage,
                            BuildingSceneObjectStorage buildingSceneObjectStorage,
                            IMonoPool monoPool)
        {
            _unitViewStorage = unitViewStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _monoPool = monoPool;
        }

        protected override void ExecuteInheritor(params object[] args)
        {
            base.ExecuteInheritor(args);
            var unitGuid = (string) args[0];
            var buildingGuid = (string) args[1];

            _projectileTarget = _buildingSceneObjectStorage.Get(buildingGuid).transform.Find(_projectileTargetName);
            _projectilePoint = _unitViewStorage.Get(unitGuid).transform.Find(_projectilePointName);
            Animator.OnAnimationEvent += OnAnimationEvent;
        }

        private void OnAnimationEvent(AnimKey anim, Event animEvent)
        {
            if (SimulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted || animEvent == null) return;

            if (anim == CurrentTrain && animEvent.Data.Name == "Shoot")
            {
                var arrow = _monoPool.Spawn<UnitArrow>();
                arrow.Init(_projectilePoint.position, _projectileTarget);
            }
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                Animator.OnAnimationEvent -= OnAnimationEvent;
            }

            _projectilePoint = null;
            _projectileTarget = null;
            _unitViewStorage = null;
            _buildingSceneObjectStorage = null;
            _monoPool = null;
            base.OnDisposed();
        }
    }
}