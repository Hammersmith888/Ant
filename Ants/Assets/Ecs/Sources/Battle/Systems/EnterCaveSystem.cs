using System.Collections.Generic;
using BugsFarm.Model.Enum;
using Entitas;

namespace Ecs.Sources.Battle.Systems
{
    public class EnterCaveSystem : ReactiveSystem<BattleEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleContext _battle;

        public EnterCaveSystem(
            AntContext ant,
            BattleContext battle) : base(battle)
        {
            _ant = ant;
            _battle = battle;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.EnterCave;

        protected override void Execute(List<BattleEntity> entities)
        {
            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.InBattle)
                .NoneOf(AntMatcher.Disabled, AntMatcher.Dead));
            
            foreach (var entity in playerGroup.GetEntities())
                entity.isEnterToCave = true;
            
            Refs.Instance.MusicBattle.FadeMute(false);

            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            battleRoom.Stock.gameObject.SetActive(false);
        }
    }
}