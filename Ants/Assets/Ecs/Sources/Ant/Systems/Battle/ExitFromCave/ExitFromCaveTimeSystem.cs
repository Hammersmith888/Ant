using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.ExitFromCave
{
    public class ExitFromCaveTimeSystem : IExecuteSystem
    {
        private readonly BattleContext _battle;
        private readonly IGroup<AntEntity> _group;

        public ExitFromCaveTimeSystem(AntContext ant,
            BattleContext battle)
        {
            _battle = battle;
            _group = ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.ExitFromCave, AntMatcher.Player)
                .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));
        }

        public void Execute()
        {
            //if (_battle.season.Value != 2)
            //    return;

            foreach (var entity in _group.GetEntities())
            {
                var battleRoom = _battle.currentRoomEntity.battleRoom.Value;

                var distance = Vector3.Distance(
                    entity.transform.Value.position, 
                    battleRoom.ExitFromCaveStart.position);
                
                var targetDistance = 0f;
                if (_battle.season.Value == 2)
                    targetDistance = 1.62f;
                else
                    targetDistance = 2f;
                
                if (distance >= targetDistance &&
                    entity.mbUnit.Value.Spine.maskInteraction == SpriteMaskInteraction.VisibleInsideMask)
                {
                    entity.mbUnit.Value.Spine.maskInteraction = SpriteMaskInteraction.None;
                }
            }
        }
    }
}