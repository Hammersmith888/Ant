using System.Collections.Generic;
using System.Linq;
using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.ExitFromCave
{
    public class ExitFromCaveAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;

        public ExitFromCaveAddedSystem(AntContext ant,
            BattleContext battle,
            BattleSettingsInstaller.AntSettings antSettings) : base(ant)
        {
            _ant = ant;
            _battle = battle;
            _antSettings = antSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.ExitFromCave.Added());

        protected override bool Filter(AntEntity entity)
            => entity.isExitFromCave;

        protected override void Execute(List<AntEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            var index = 0;

            entities = entities.OrderByDescending(item => item.antType.Value).ToList();

            foreach (var entity in entities)
            {
                if (entity.hasArcherView)
                {
                    foreach (var arrowView in entity.archerView.Value.Arrows.ToArray())
                        Object.Destroy(arrowView.gameObject);

                    entity.archerView.Value.Arrows.Clear();
                }

                entity.renderer.Value.material.SetFloat("_FillPhase", 1f);
                entity.AddFadeIn(0f);
                entity.ReplaceEnemiesCount(0);

                entity.skeletonAnimation.Value.timeScale = _antSettings.animationScaleExitFromCave;

                var position = battleRoom.ExitFromCaveStart.position;
                position.x += _antSettings.antStartOffset * index;
                entity.transform.Value.position = position;

                var destination = battleRoom.ExitFromCaveEnd.position;
                destination.x += _antSettings.antStartOffset * index;
                entity.ReplaceMoveToPosition(destination);

                entity.mbUnit.Value.Spine.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;

                index++;
            }

            // chest
            var chestEntity = _ant.GetEntityWithChestRoomIndex(currentRoom.roomIndex.Value);
            if (!chestEntity.hasFadeIn)
                chestEntity.AddFadeIn(0f);
        }
    }
}