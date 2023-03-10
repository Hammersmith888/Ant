using System.Globalization;
using BugsFarm.AnimationsSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.SpeakerSystem;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class AntLarvaGrowthTask : LarvaGrowthTask
    {
        private const string _birthModelIDStatKey = "stat_birthModelID";
        private readonly CustomAnimationPlayer _customAnimation;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly SceneEntityStorage _sceneEntityStorage;
        private readonly AnimationModelStorage _animationModelStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly ISpeakerSystem _speakerSystem;
        
        private IUnitMover _unitMover;
        private string _entityTypeName;

        public AntLarvaGrowthTask(CustomAnimationPlayer customAnimation,
                                  UnitSceneObjectStorage unitSceneObjectStorage,
                                  SceneEntityStorage sceneEntityStorage,
                                  AnimationModelStorage animationModelStorage,
                                  UnitMoverStorage unitMoverStorage,
                                  ISpeakerSystem speakerSystem)
        {
            _customAnimation = customAnimation;
            _unitSceneObjectStorage = unitSceneObjectStorage;
            _sceneEntityStorage = sceneEntityStorage;
            _animationModelStorage = animationModelStorage;
            _unitMoverStorage = unitMoverStorage;
            _speakerSystem = speakerSystem;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;
            base.Execute(args);
            _entityTypeName = _sceneEntityStorage.Get(UnitId).GetType().Name;
            _unitMover = _unitMoverStorage.Get(UnitId);
        }

        protected override void OnGrowthStageEnd()
        {
            base.OnGrowthStageEnd();
            if (Finalized) return;
            UpdateSpeaker();
            if (StageCount == 1)
            {
                _customAnimation.Play();
            }
            else
            {
                _customAnimation.Stop();
                var animModelFromStage = _animationModelStorage.Get(GetAnimationModelID());
                _customAnimation.Animator.ChangeModel(animModelFromStage);
                _customAnimation.Animator.SetAnim(AnimKey.Idle);
            }
        }

        private string GetAnimationModelID()
        {
            if (Finalized) return "";
            return _entityTypeName + "_" + (Mathf.Max(0, StageCount - 1));
        }

        private void UpdateSpeaker()
        {
            if (Finalized) return;
            if (StageCount == 1)
            {
                if (_speakerSystem.HasEntity(UnitId)) return;

                var view = _unitSceneObjectStorage.Get(UnitId);
                _speakerSystem.Registration(UnitId, _entityTypeName + StageCount, view.transform);
                _speakerSystem.ChangeState(UnitId, PhraseState.idle);
            }
            else
            {
                _speakerSystem.UnRegistration(UnitId);
            }
        }

        protected override void OnDisposed()
        {
            _customAnimation?.Stop();
            _speakerSystem?.UnRegistration(UnitId);
            _statsCollection = null;
            _unitMover = null;
            base.OnDisposed();
        }

        protected override void OnCompleted()
        {
            var unitBuildingProtocol = new CreateUnitProtocol(_statsCollection.GetValue(_birthModelIDStatKey)
                                                                  .ToString(CultureInfo.InvariantCulture), true);
            Instantiator.Instantiate<CreateUnitCommand>().Execute(unitBuildingProtocol);

            var spawnProtocol = new UnitSpawnProtocol(unitBuildingProtocol.Guid, _unitMover.Position);
            Instantiator.Instantiate<UnitSpawnCommand<SpawnFromAlphaTask>>().Execute(spawnProtocol);
            
            var protocol = new DeleteUnitProtocol(UnitId);
            var command = Instantiator.Instantiate<DeleteUnitCommand>();
            command.Execute(protocol);
            
            base.OnCompleted();
        }
    }
}