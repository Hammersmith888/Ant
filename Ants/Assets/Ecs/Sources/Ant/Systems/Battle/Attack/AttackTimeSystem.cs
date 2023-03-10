using System;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;
using UniRx;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Attack
{
    public class AttackTimeSystem : IExecuteSystem, IDisposable
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly IGroup<AntEntity> _group;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly UnitFightSettingsInstaller.UnitFightSettings _unitFightSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public AttackTimeSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService,
            BattleSettingsInstaller.BattleSettings battleSettings,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings,
            UnitFightSettingsInstaller.UnitFightSettings unitFightSettings)
        {
            _ant = ant;
            _antSettings = antSettings;
            _antService = antService;
            _battleSettings = battleSettings;
            _unitViewSettings = unitViewSettings;
            _unitFightSettings = unitFightSettings;
            _group = ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.AttackTime, AntMatcher.AttackTimeMax)
                .NoneOf(AntMatcher.Dead));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var attackTime = entity.attackTime.Value;

                attackTime += Time.deltaTime;
                if (attackTime >= entity.attackTimeMax.Value)
                    Attack(entity);
                else
                    entity.ReplaceAttackTime(attackTime);
            }
        }

        private void Attack(AntEntity entity)
        {
            if (!entity.hasTarget || entity.isDead)
                return;


            if (!entity.isRangedUnit)
                AttackMelee(entity);
            else
                entity.isShoot = true;
        }

        private void AttackMelee(AntEntity entity)
        {
            var antVo = _antService.GetAntVo(entity.antType.Value);
            var targetUid = entity.target.Uid;
            var targetEntity = _ant.GetEntityWithUid(targetUid);
            var config = _unitViewSettings.units[entity.antType.Value];

            Observable.Timer(TimeSpan.FromSeconds(0.2f))
                .Subscribe(_ =>
                {
                    if (entity.isDead || !entity.isAttackMelee)
                        return;

                    var fightItemConfig = _unitFightSettings.units[entity.antType.Value]
                        .levels[entity.level.Value];
                    if (!targetEntity.hasDamage)
                    {
                        var damage = fightItemConfig.attack;
                        if (entity.isAttackBoost)
                            damage *= _battleSettings.attackRate;
                        targetEntity.AddDamage(damage);
                    }
                    else
                    {
                        var damage = targetEntity.damage.Value;
                        damage += fightItemConfig.attack;
                        if (entity.isAttackBoost)
                            damage *= _battleSettings.attackRate;
                        targetEntity.ReplaceDamage(damage);
                    }
                })
                .AddTo(_disposable);

            entity.ReplaceAttackTime(0f);
            entity.skeletonAnimation.Value.timeScale = config.animationScale.attack;

            var animationName = "";
            if (entity.isPlayer)
            {
                if (entity.antType.Value == AntType.Pikeman)
                {
                    if (entity.id.Value % 2 == 0)
                    {
                        animationName = "attack";
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "attack", false);
                    }
                    else
                    {
                        animationName = "attack2";
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "attack2", false);
                    }
                }
                else if (entity.antType.Value == AntType.Worker)
                {
                    if (entity.id.Value % 2 == 0)
                    {
                        animationName = "Fight_lopata_1";
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Fight_lopata_1", false);
                    }
                    else
                    {
                        animationName = "Fight_lopata_2";
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Fight_lopata_2", false);
                    }
                }
                else if (entity.antType.Value == AntType.Snail)
                {
                    //entity.skeletonAnimation.Value.state.SetAnimation(0, "Damage", false);
                }
                else if (entity.antType.Value == AntType.Spider ||
                         entity.antType.Value == AntType.Swordman ||
                         entity.antType.Value == AntType.Firefly ||
                         entity.antType.Value == AntType.Butterfly ||
                         entity.antType.Value == AntType.LadyBug)
                {
                    animationName = "Attack";
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Attack", false);
                }
                else
                {
                    animationName = "attack";
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "attack", false);
                }
            }
            else
            {
                if (entity.antType.Value == AntType.Bedbug)
                {
                    animationName = "Attcak";
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Attcak", false);
                }
                else
                {
                    animationName = "Attack";
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Attack", false);
                }
            }

            entity.isAttacking = true;
            CheckFlip(entity);

            var attackAnimationTime = 0f;
            if (animationName.Length > 0)
            {
                var myAnimation = entity.skeletonAnimation.Value.Skeleton.Data.FindAnimation(animationName);
                attackAnimationTime = myAnimation.Duration;
            }
            else
                attackAnimationTime = antVo.attackAnimationTime;

            Observable.Timer(TimeSpan.FromSeconds(attackAnimationTime))
                .Subscribe(
                    _ =>
                    {
                        entity.isAttacking = false;
                        if (entity.isDead || !entity.isAttackMelee)
                            return;

                        entity.skeletonAnimation.Value.timeScale = config.animationScale.idle;
                        if (entity.isPlayer)
                            entity.skeletonAnimation.Value.AnimationName = "breath";
                        else if (entity.antType.Value == AntType.CaterpillarBoss)
                            entity.skeletonAnimation.Value.AnimationName = "idle";
                        else if (entity.antType.Value == AntType.Mosquito)
                            entity.skeletonAnimation.Value.AnimationName = "Run";
                        else
                            entity.skeletonAnimation.Value.AnimationName = "Idle";
                    })
                .AddTo(_disposable);
        }

        private void CheckFlip(AntEntity entity)
        {
            var target = _ant.GetEntityWithUid(entity.target.Uid);
            var destination = target.transform.Value.position;

            if (entity.antType.Value == AntType.PotatoBug
                || entity.antType.Value == AntType.Worm
                || entity.antType.Value == AntType.Cockroach
                || entity.antType.Value == AntType.CaterpillarBoss
                || entity.antType.Value == AntType.Bedbug
                || entity.antType.Value == AntType.Fly
                || entity.antType.Value == AntType.MolBoss
                || entity.antType.Value == AntType.Mosquito)
            {
                if (destination.x < entity.transform.Value.position.x)
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
                else
                    entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
            }
            else
            {
                if (destination.x < entity.transform.Value.position.x)
                    entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
                else
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
            }
        }

        public void Dispose() => _disposable?.Dispose();
    }
}