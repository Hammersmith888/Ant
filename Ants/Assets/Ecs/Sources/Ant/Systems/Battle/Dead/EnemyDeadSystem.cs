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
    public class EnemyDeadSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly AntContext _ant;
        private readonly FightScreen _fightScreen;
        private readonly BattleSettingsInstaller.BattleSettings battleSettings;
        private readonly BattleContext _battle;
        private readonly BattleService _battleService;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public EnemyDeadSystem(AntContext ant,
            FightScreen fightScreen,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleContext battle,
            BattleService battleService,
            BattleSettingsInstaller.AntSettings antSettings
        ) : base(ant)
        {
            _ant = ant;
            _fightScreen = fightScreen;
            this.battleSettings = battleSettings;
            _battle = battle;
            _battleService = battleService;
            _antSettings = antSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Dead);

        protected override bool Filter(AntEntity entity)
            => entity.isDead && entity.isEnemy;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.mbUnit.Value.Collider.enabled = false;
                
                if (entity.antType.Value == AntType.PotatoBug)
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Dead", false);
                else
                    entity.skeletonAnimation.Value.state.SetAnimation(0, "Death", false);

                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();

                RemoveAttackers(entity);
            }

            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            if (enemyGroup.count == 0)
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

            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
            var group = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Target)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
            foreach (var groupEntity in group.GetEntities())
            {
                if (groupEntity.target.Uid == entity.uid.Value)
                {
                    groupEntity.RemoveTarget();
                    if (enemyGroup.count > 0)
                        _battleService.SetNextTarget(groupEntity);
                }
            }
        }

        private void EndBattle()
        {
            _battle.isBattleFinish = true;
            if (_ant.selectedEntity != null)
                _ant.selectedEntity.isSelected = false;

            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

            foreach (var entity in playerGroup.GetEntities())
            {
                if (entity.hasTarget)
                {
                    entity.RemoveTarget();
                    entity.isMoveToTarget = false;
                }

                var mb = entity.mbUnit.Value;
                mb.HpBar.gameObject.SetActive(false);

                entity.isAttackMelee = false;
                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();
                if (entity.hasMoveToPosition)
                    entity.RemoveMoveToPosition();
            }

            Observable.Timer(TimeSpan.FromSeconds(battleSettings.openChestDelay))
                .Subscribe(_ =>
                {
                    var currentRoom = _battle.currentRoomEntity;
                    var battleRoom = currentRoom.battleRoom.Value;

                    battleRoom.Chest.gameObject.SetActive(false);

                    _fightScreen.WinPanel.Open();
                    Refs.Instance.MusicBattle.FadeMute(true);
                    Sounds.Play(Sound.JingleVictory);
                })
                .AddTo(_disposable);
        }

        public void Dispose() => _disposable?.Dispose();
    }
}