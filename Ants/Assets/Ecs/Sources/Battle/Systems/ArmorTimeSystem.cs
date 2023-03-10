using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class ArmorTimeSystem : IExecuteSystem
    {
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly AntContext _ant;

        public ArmorTimeSystem(BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings,
            AntContext ant)
        {
            _battle = battle;
            _battleSettings = battleSettings;
            _ant = ant;
        }

        public void Execute()
        {
            if (!_battle.isArmorBoost)
                return;

            var time = _battle.armorTime.Value;

            time += Time.deltaTime;
            if (time >= _battleSettings.armorDuration)
            {
                _battle.isArmorBoost = false;
                _battle.RemoveArmorTime();
                
                var group = _ant.GetGroup(AntMatcher
                    .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                    .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

                foreach (var entity in group.GetEntities())
                    entity.isArmorBoost = false;
            }
            else
            {
                _battle.ReplaceArmorTime(time);
            }
        }
    }
}