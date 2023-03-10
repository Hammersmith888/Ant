using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using Entitas;

namespace Ecs.Sources.Battle.Systems
{
    public class ExitFromCaveSystem : ReactiveSystem<BattleEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleContext _battle;

        public ExitFromCaveSystem(
            AntContext ant,
            BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings
        ) : base(battle)
        {
            _ant = ant;
            _battle = battle;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.ExitFromCave;

        protected override void Execute(List<BattleEntity> entities)
        {
            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Disabled, AntMatcher.Dead));

            foreach (var entity in playerGroup.GetEntities())
            {
                entity.isExitFromCave = true;
                entity.ReplaceHealth(entity.healthMax.Value);
            }

            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy)
                .NoneOf(AntMatcher.Disabled, AntMatcher.Dead));

            foreach (var entity in enemyGroup.GetEntities())
            {
                entity.isPatrol = false;
                entity.isMoveToStartPosition = true;
            }
        }
    }
}