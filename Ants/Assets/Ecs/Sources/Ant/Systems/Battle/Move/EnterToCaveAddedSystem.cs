using System.Collections.Generic;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Move
{
    public class EnterToCaveAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleContext _battle;

        public EnterToCaveAddedSystem(AntContext ant,
            BattleContext battle) : base(ant)
        {
            _ant = ant;
            _battle = battle;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.EnterToCave);

        protected override bool Filter(AntEntity entity)
            => entity.isEnterToCave;

        protected override void Execute(List<AntEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;

            foreach (var entity in entities)
            {
                entity.AddMoveToPosition(battleRoom.EnterToCaveEnd.position);

                if (entity.skeletonAnimation.Value.skeleton.ScaleX < 0)
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
            }

            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Ant, AntMatcher.Dead)
                .NoneOf(AntMatcher.Disabled));
            foreach (var entity in enemyGroup.GetEntities())
                entity.mbUnit.Value.gameObject.SetActive(false);
        }
    }
}