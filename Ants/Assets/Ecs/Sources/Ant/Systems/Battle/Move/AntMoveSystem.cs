using System;
using BugsFarm.Installers;
using BugsFarm.Services;
using DesperateDevs.Utils;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Move
{
    public class AntMoveSystem : IExecuteSystem
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly BattleContext _battle;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly IGroup<AntEntity> _group;

        public AntMoveSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService,
            BattleContext battle,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings)
        {
            _ant = ant;
            _antSettings = antSettings;
            _antService = antService;
            _battle = battle;
            _unitViewSettings = unitViewSettings;
            _group = ant.GetGroup(AntMatcher
                .AnyOf(AntMatcher.MoveToTarget, AntMatcher.MoveToPosition)
                .NoneOf(AntMatcher.WaitOpponent, AntMatcher.Dead));
        }

        public void Execute()
        {
            foreach (var entity in _group.GetEntities())
            {
                var transform = entity.transform.Value;
                Vector3 destination;

                if (entity.isMoveToTarget)
                {
                    if (!entity.hasTarget)
                    {
                        entity.isMoveToTarget = false;
                        return;
                    }

                    var target = _ant.GetEntityWithUid(entity.target.Uid);
                    destination = target.transform.Value.position;
                }
                else if (entity.hasMoveToPosition)
                    destination = entity.moveToPosition.Position;
                else
                    throw new Exception("Error!");

                var direction = (destination - transform.position).normalized;
                var speed = 0f;
                var config = _unitViewSettings.units[entity.antType.Value];

                if (entity.isExitFromCave)
                    speed = config.walkSpeed;
                else if (entity.isMoveToStartPosition)
                    speed = config.moveToStartSpeed;
                else
                    speed = config.moveSpeed;
                transform.Translate(direction * (Time.deltaTime * speed));

                // todo hack
                //
                if (entity.isEnterToCave || entity.isRetreat)
                    SetFog(entity);
                if (entity.isExitFromCave && entity.hasFadeIn)
                    entity.renderer.Value.material.SetFloat("_FillPhase", 1f);
                //

                var distance = Vector3.Distance(destination, transform.position);
                var delta = 0f;

                if (entity.isMoveToTarget)
                {
                    var target = _ant.GetEntityWithUid(entity.target.Uid);
                    var antVo = _antService.GetAntVo(target.antType.Value);
                    delta = antVo.attackDistance;

                    if (entity.antType.Value == AntType.CaterpillarBoss || antVo.type == AntType.CaterpillarBoss)
                        delta *= 1.5f;
                }
                else
                    delta = _antSettings.antMoveDeltaStop;

                if (distance <= delta)
                {
                    if (entity.isMoveToTarget)
                    {
                        entity.isMoveToTarget = false;

                        var target = _ant.GetEntityWithUid(entity.target.Uid);

                        if (entity.isPlayer)
                        {
                            if (entity.antType.Value == AntType.Snail)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle_1", true);
                            else if (entity.antType.Value == AntType.Spider ||
                                     entity.antType.Value == AntType.Swordman ||
                                     entity.antType.Value == AntType.Firefly ||
                                     entity.antType.Value == AntType.LadyBug)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle", true);
                            else if (entity.antType.Value == AntType.Butterfly)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Fly", true);
                            else
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "breath", true);
                        }
                        else
                        {
                            if (entity.antType.Value == AntType.Mosquito)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Run", true);
                            else if (entity.antType.Value != AntType.CaterpillarBoss &&
                                     entity.antType.Value != AntType.MolBoss)
                                entity.skeletonAnimation.Value.state.SetAnimation(0, "Idle", true);
                        }

                        entity.isAttackMelee = true;
                        if (!target.hasMoveToPosition)
                            target.isAttackMelee = true;
                    }

                    if (entity.hasMoveToPosition)
                    {
                        entity.RemoveMoveToPosition();
                        entity.skeletonAnimation.Value.AnimationName = entity.isPlayer ? "breath" : "Idle";
                    }

                    //
                    if (entity.hasFadeOut)
                        entity.RemoveFadeOut();
                }
            }
        }

        private void SetFog(AntEntity entity)
        {
            if (!entity.hasMoveToPosition)
                return;

            var destination = entity.moveToPosition.Position;
            var transform = entity.transform.Value;
            var distance = Vector3.Distance(destination, transform.position);
            var fadeOutDistance = entity.isRetreat
                ? _antSettings.fadeOutDistanceRetreat
                : _antSettings.fadeOutDistance;

            if (!entity.isRetreat)
            {
                fadeOutDistance = _battle.season.Value == 1
                    ? _antSettings.fadeOutDistanceSeason1
                    : _antSettings.fadeOutDistanceSeason2;
            }

            if (distance <= fadeOutDistance)
            {
                var renderer = entity.renderer.Value;
                var fadeOutTime = 0f;
                if (entity.hasFadeOut)
                    fadeOutTime = entity.fadeOut.Time;

                fadeOutTime += Time.deltaTime;
                entity.ReplaceFadeOut(fadeOutTime);

                if (fadeOutTime >= 0f && fadeOutTime <= 1f)
                    renderer.material.SetFloat("_FillPhase", Mathf.Clamp01(fadeOutTime));

                var targetDistance = 0f;
                if (_battle.season.Value == 2)
                    targetDistance = 1.8f;
                else
                    targetDistance = 4f;
                if (distance <= targetDistance &&
                    entity.mbUnit.Value.Spine.maskInteraction == SpriteMaskInteraction.None)
                {
                    entity.mbUnit.Value.Spine.maskInteraction = SpriteMaskInteraction.VisibleInsideMask;
                }
            }
        }
    }
}