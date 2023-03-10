using System;
using System.Collections.Generic;
using BugsFarm.Services;
using Entitas;
using UniRx;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Target
{
    public class TargetAddedMeleeSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly AntContext _ant;
        private readonly AntService _antService;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public TargetAddedMeleeSystem(AntContext ant,
            AntService antService) : base(ant)
        {
            _ant = ant;
            _antService = antService;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Target.Added());

        protected override bool Filter(AntEntity entity)
            => entity.hasTarget && entity.antType.Value != AntType.Archer;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var targetGroup = _ant.GetGroup(AntMatcher
                    .AllOf(AntMatcher.Target, AntMatcher.AttackMelee)
                    .NoneOf(AntMatcher.Dead, AntMatcher.Disabled));

                var hasAttackers = false;
                foreach (var groupEntity in targetGroup.GetEntities())
                {
                    if (groupEntity.uid.Value == entity.uid.Value)
                        continue;

                    if (groupEntity.uid.Value == entity.target.Uid)
                        hasAttackers = true;
                }

                if (!hasAttackers)
                {
                    if (!entity.hasMoveToPosition)
                        entity.isMoveToTarget = true;

                    if (entity.antType.Value == AntType.Worm)
                    {
                        Observable.Timer(TimeSpan.FromSeconds(_antService.GetAntVo(entity.antType.Value).stopTime))
                            .Subscribe(_ => CheckWaitOpponent(entity))
                            .AddTo(_disposable);
                    }
                }
                else
                {
                    if (entity.isPlayer)
                        entity.isFindAttackPosition = true;
                    else if (!entity.hasMoveToPosition)
                        entity.isMoveToTarget = true;
                }
            }
        }

        private void CheckWaitOpponent(AntEntity entity)
        {
            /*if (entity.health.Value == entity.healthMax.Value)
            {
                var meleeGroup = _ant.GetGroup(AntMatcher
                    .AllOf(AntMatcher.Player)
                    .NoneOf(AntMatcher.Disabled, AntMatcher.Dead, AntMatcher.Destroy, AntMatcher.RangedUnit));

                if (meleeGroup.count > 0)
                {
                    entity.isWaitOpponent = true;
                    entity.skeletonAnimation.Value.AnimationName = "Idle";
                }
            }*/
        }

        /*private static void CheckFlip(AntEntity entity, Vector3 destination)
        {
            if (entity.antType.Value == AntType.PotatoBug
                || entity.antType.Value == AntType.Worm
                || entity.antType.Value == AntType.Cockroach)
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
        }*/

        public void Dispose() => _disposable?.Dispose();
    }
}