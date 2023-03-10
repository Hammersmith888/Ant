using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Move
{
    public class MoveToStartPositionAdded : ReactiveSystem<AntEntity>
    {
        private readonly BattleContext _battle;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public MoveToStartPositionAdded(IContext<AntEntity> context,
            BattleContext battle,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(context)
        {
            _battle = battle;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.MoveToStartPosition);

        protected override bool Filter(AntEntity entity)
            => entity.isMoveToStartPosition && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            var index = 0;

            foreach (var entity in entities)
            {
                var destination = Vector3.zero;
                if (entities.Count == 1)
                {
                    destination = battleRoom.EnemyStartPoints[2].position;
                }
                else if (entities.Count == 2)
                {
                    destination = battleRoom.EnemyStartPoints[index].position;
                }
                else if (entities.Count == 3)
                {
                    destination = battleRoom.EnemyStartPoints[index].position;
                }
                else
                    throw new NotImplementedException();

                entity.ReplaceMoveToPosition(destination);

                var config = _unitViewSettings.units[entity.antType.Value];
                entity.skeletonAnimation.Value.timeScale =
                    entity.isPlayer ? config.animationScale.walk : config.animationScale.run;

                index++;
            }
        }
    }
}