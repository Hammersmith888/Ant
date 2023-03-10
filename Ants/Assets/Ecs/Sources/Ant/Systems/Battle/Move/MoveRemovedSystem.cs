using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using Entitas;
using UniRx;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

namespace Ecs.Sources.Ant.Systems.Battle.Move
{
    public class MoveRemovedSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly AntContext _ant;
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly BattleService _battleService;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public MoveRemovedSystem(AntContext ant,
            BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleService battleService,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(ant)
        {
            _ant = ant;
            _battle = battle;
            _battleSettings = battleSettings;
            _battleService = battleService;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.AnyOf(
                AntMatcher.MoveToTarget, AntMatcher.MoveToPosition).Removed());

        protected override bool Filter(AntEntity entity)
            => !entity.isMoveToTarget && !entity.hasMoveToPosition && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (!entity.hasAntType)
                    return;

                var config = _unitViewSettings.units[entity.antType.Value];
                entity.skeletonAnimation.Value.timeScale = config.animationScale.idle;

                switch (entity.antType.Value)
                {
                    case AntType.Spider:
                    case AntType.Swordman:
                    case AntType.Cockroach:
                    case AntType.PotatoBug:
                    case AntType.Worm:
                    case AntType.Bedbug:
                    case AntType.Fly:
                    case AntType.MolBoss:
                    case AntType.Firefly:
                    case AntType.LadyBug:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle", true);
                        break;
                    case AntType.Butterfly:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Fly", true);
                        break;
                    case AntType.Worker:
                    case AntType.Pikeman:
                    case AntType.Archer:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "breath", true);
                        break;
                    case AntType.Snail:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle_2", true);
                        break;
                    case AntType.CaterpillarBoss:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "idle", true);
                        break;
                    case AntType.Mosquito:
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Run", true);
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (entity.isPlayer && _battle.fightState.Value == EFightState.ExitFromCave)
                    _battle.ReplaceFightState(EFightState.Logo);

                if (entity.isPatrol)
                {
                    var waitTime = Random.Range(_battleSettings.patrolWait.x, _battleSettings.patrolWait.y);
                    entity.AddWaitPatrol(0f, waitTime);
                }

                if (entity.hasTarget)
                {
                    var target = entity.target.Transform;
                    var transform = entity.transform.Value;

                    if (entity.isPlayer)
                    {
                        if (target.position.x < transform.position.x)
                            entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
                        else
                            entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
                    }
                    else
                    {
                        if (target.position.x < transform.position.x)
                            entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
                        else
                            entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
                    }
                }

                if (entity.isFindAttackPosition && !entity.hasMoveToPosition)
                {
                    entity.isFindAttackPosition = false;
                    entity.isAttackMelee = true;
                }

                if (entity.isExitFromCave)
                    entity.isExitFromCave = false;

                if (entity.isEnterToCave && entity.isPlayer)
                {
                    entity.isEnterToCave = false;

                    var group = _ant.GetGroup(AntMatcher
                        .AllOf(AntMatcher.EnterToCave, AntMatcher.MoveToPosition));
                    if (group.count == 0)
                        _battle.ReplaceFightState(EFightState.CameraTween);
                }

                if (entity.isMoveToStartPosition)
                {
                    entity.isMoveToStartPosition = false;
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
                }

                if (entity.isRetreat)
                {
                    entity.isRetreat = false;
                    CheckRetreat();
                }
            }
        }

        private void CheckRetreat()
        {
            var group = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle, AntMatcher.Retreat)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            if (group.count == 0)
                RestartScene();
        }

        private void RestartScene()
        {
            _battleService.ClearEntities();
            _battle.ReplaceFightState(EFightState.None);

            Observable.NextFrame()
                .Subscribe(_ => SceneManager.LoadScene(SceneManager.GetActiveScene().name))
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable?.Dispose();
    }
}