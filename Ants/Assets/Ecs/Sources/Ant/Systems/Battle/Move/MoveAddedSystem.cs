using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Move
{
    public class MoveAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public MoveAddedSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(ant)
        {
            _ant = ant;
            _antSettings = antSettings;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.AnyOf(
                AntMatcher.MoveToTarget, AntMatcher.MoveToPosition).Added());

        protected override bool Filter(AntEntity entity)
            => (entity.isMoveToTarget || entity.hasMoveToPosition) && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                // fix change scale
                var mbUnit = entity.mbUnit.Value;
                if (mbUnit.SpineParent != null)
                    mbUnit.SpineParent.localScale = Vector3.one;
                var spineTransform = mbUnit.Spine.transform;
                if (spineTransform.localScale != Vector3.one)
                    spineTransform.localScale = Vector3.one;
                //

                var config = _unitViewSettings.units[entity.antType.Value];
                // run
                if (!entity.isExitFromCave)
                {
                    entity.skeletonAnimation.Value.timeScale = config.animationScale.run;

                    switch (entity.antType.Value)
                    {
                        case AntType.Spider:
                        case AntType.Firefly:
                        case AntType.LadyBug:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Walk", true);
                            break;
                        case AntType.Snail:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Walk_Fight", true);
                            break;
                        case AntType.PotatoBug:
                        case AntType.Worm:
                        case AntType.Bedbug:
                        case AntType.Fly:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Run", true);
                            break;
                        case AntType.Swordman:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Run2", true);
                            break;
                        case AntType.Worker:
                        case AntType.Archer:
                        case AntType.Pikeman:
                        case AntType.Cockroach:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "run", true);
                            break;
                        case AntType.CaterpillarBoss:
                        case AntType.MolBoss:
                            break;
                        case AntType.Mosquito:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Run", true);
                            break;
                        case AntType.Butterfly:
                            entity.skeletonAnimation.Value.state.SetAnimation(0, "Fly", true);
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                }
                else // walk
                {
                    entity.skeletonAnimation.Value.timeScale = config.animationScale.walk;
                    
                    if (entity.antType.Value == AntType.Spider || entity.antType.Value == AntType.Swordman ||
                        entity.antType.Value == AntType.Firefly || entity.antType.Value == AntType.LadyBug)
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Walk", true);
                    else if (entity.antType.Value == AntType.Snail)
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Walk_Fight", true);
                    else if (entity.antType.Value == AntType.Butterfly)
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "Fly", true);
                    else
                        entity.skeletonAnimation.Value.state.SetAnimation(0, "walk", true);
                }

                entity.isAttackMelee = false;
                if (entity.hasAttackTime)
                    entity.RemoveAttackTime();
                if (entity.hasAttackTimeMax)
                    entity.RemoveAttackTimeMax();

                Vector3 destination;
                if (entity.isMoveToTarget)
                {
                    var target = _ant.GetEntityWithUid(entity.target.Uid);
                    destination = target.transform.Value.position;
                }
                else if (entity.hasMoveToPosition)
                    destination = entity.moveToPosition.Position;
                else
                    throw new Exception("Error!");

                CheckFlip(entity, destination);
            }
        }

        private static void CheckFlip(AntEntity entity, Vector3 destination)
        {
            if (entity.antType.Value == AntType.PotatoBug
                || entity.antType.Value == AntType.Worm
                || entity.antType.Value == AntType.Cockroach
                || entity.antType.Value == AntType.CaterpillarBoss
                || entity.antType.Value == AntType.Bedbug
                || entity.antType.Value == AntType.Fly
                || entity.antType.Value == AntType.MolBoss
                || entity.antType.Value == AntType.Mosquito)
            {
                if (destination.x < entity.transform.Value.position.x)
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
                else
                    entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
            }
            else
            {
                if (destination.x < entity.transform.Value.position.x)
                    entity.skeletonAnimation.Value.skeleton.ScaleX = -1f;
                else
                    entity.skeletonAnimation.Value.skeleton.ScaleX = 1f;
            }
        }
    }
}