using System.Collections.Generic;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Retreat
{
    public class RetreatSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleContext _battle;

        public RetreatSystem(IContext<AntEntity> context,
            BattleContext battle) : base(context)
        {
            _battle = battle;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Retreat);

        protected override bool Filter(AntEntity entity)
            => entity.isRetreat;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();
                entity.isAttacking = false;
                entity.isAttackMelee = false;
                entity.isFindAttackPosition = false;
                if (entity.hasTarget)
                    entity.RemoveTarget();
                if (entity.hasAttackers)
                    entity.RemoveAttackers();

                entity.mbUnit.Value.HpBar.gameObject.SetActive(false);

                if (entity.isPlayer)
                {
                    var currentRoom = _battle.currentRoomEntity;
                    var battleRoom = currentRoom.battleRoom.Value;

                    entity.ReplaceMoveToPosition(battleRoom.ExitFromCaveStart.position);
                    entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
                }
                else
                {
                    entity.skeletonAnimation.Value.AnimationName = "Idle";
                }
            }
        }
    }
}