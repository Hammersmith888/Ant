using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AudioSystem;
using BugsFarm.AudioSystem.Obsolete;
using BugsFarm.Installers;
using BugsFarm.Services;
using BugsFarm.Views.Screen;
using Entitas;
using UniRx;

namespace Ecs.Sources.Ant.Systems.Battle.Dead
{
    public class PlayerDeadSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly AntContext _ant;
        private readonly FightScreen _fightScreen;
        private readonly BattleSettingsInstaller.BattleSettings battleSettings;
        private readonly BattleContext _battle;
        private readonly BattleService _battleService;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public PlayerDeadSystem(AntContext ant,
            FightScreen fightScreen,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleContext battle,
            BattleService battleService,
            BattleSettingsInstaller.AntSettings antSettings,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings
        ) : base(ant)
        {
            _ant = ant;
            _fightScreen = fightScreen;
            this.battleSettings = battleSettings;
            _battle = battle;
            _battleService = battleService;
            _antSettings = antSettings;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Dead);

        protected override bool Filter(AntEntity entity)
            => entity.isDead && entity.isPlayer;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var config = _unitViewSettings.units[entity.antType.Value];
                entity.skeletonAnimation.Value.timeScale = config.animationScale.death;
                
                if (entity.isSelected)
                    entity.isSelected = false;
                if (entity.antType.Value == AntType.Worker)
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "death_war", false);
                else if (entity.antType.Value == AntType.Spider || entity.antType.Value == AntType.Swordman ||
                         entity.antType.Value == AntType.Bedbug || entity.antType.Value == AntType.Butterfly)
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Death", false);
                else if (entity.antType.Value == AntType.Firefly || entity.antType.Value == AntType.LadyBug)
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Die", false);
                else if (entity.antType.Value == AntType.Snail)
                {
                    //entity.skeletonAnimation.Value.state.SetAnimation(0, "Death", false);
                }
                else
                {
                    entity.skeletonAnimation.Value.state.SetAnimation(0,
                        !entity.isRangedUnit ? "Death_world" : "Death",
                        false);
                }

                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();

                RemoveAttackers(entity);
            }

            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            if (playerGroup.count == 0)
                EndBattle();
        }

        private void RemoveAttackers(AntEntity entity)
        {
            if (entity.hasAttackers)
            {
                var attackers = entity.attackers.Uids;
                foreach (var attackerUid in attackers)
                {
                    var attacker = _ant.GetEntityWithUid(attackerUid);
                    if (attacker.hasAttackers)
                    {
                        var attackerAttackers = attacker.attackers.Uids.ToList();
                        if (attackerAttackers.Contains(entity.uid.Value))
                        {
                            attacker.ReplaceEnemiesCount(attacker.enemiesCount.Value - 1);
                            attackerAttackers.Remove(entity.uid.Value);
                        }

                        attacker.ReplaceAttackers(attackerAttackers.ToArray());
                    }
                }

                entity.RemoveAttackers();
            }

            entity.ReplaceEnemiesCount(0);

            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
            var group = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Target)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
            foreach (var groupEntity in group.GetEntities())
            {
                if (groupEntity.target.Uid == entity.uid.Value)
                {
                    groupEntity.RemoveTarget();
                    if (playerGroup.count > 0)
                        _battleService.SetNextTarget(groupEntity);
                }
            }
        }

        private void EndBattle()
        {
            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            foreach (var entity in enemyGroup.GetEntities())
            {
                entity.skeletonAnimation.Value.AnimationName = "Idle";
                if (entity.hasTarget)
                    entity.RemoveTarget();

                var mb = entity.mbUnit.Value;
                mb.HpBar.gameObject.SetActive(false);

                entity.isAttackMelee = false;
                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();
            }

            Observable.Timer(TimeSpan.FromSeconds(battleSettings.openChestDelay))
                .Subscribe(_ =>
                {
                    _fightScreen.FailPanel.Open();
                    Refs.Instance.MusicBattle.FadeMute(true);
                    Sounds.Play(Sound.JingleDefeat);
                })
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable?.Dispose();
    }
}