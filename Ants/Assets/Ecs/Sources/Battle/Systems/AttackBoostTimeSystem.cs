using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class AttackBoostTimeSystem : IExecuteSystem
    {
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly AntContext _ant;

        public AttackBoostTimeSystem(BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings,
            AntContext ant)
        {
            _battle = battle;
            _battleSettings = battleSettings;
            _ant = ant;
        }

        public void Execute()
        {
            if (!_battle.hasAttackBoost)
                return;

            var time = _battle.attackBoost.Time;

            time += Time.deltaTime;
            if (time >= _battleSettings.attackDuration)
            {
                _battle.RemoveAttackBoost();
                
                var group = _ant.GetGroup(AntMatcher
                    .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                    .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

                foreach (var entity in group.GetEntities())
                    entity.isAttackBoost = false;
            }
            else
            {
                _battle.ReplaceAttackBoost(time);
            }
        }
    }
}