using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Views.Fight;
using BugsFarm.Views.Hack;
using Entitas;
using UniRx;

namespace Ecs.Sources.Ant.Systems.Battle.Attack
{
    public class ShootSystem : ReactiveSystem<AntEntity>, IDisposable
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public ShootSystem(IContext<AntEntity> context,
            BattleSettingsInstaller.AntSettings antSettings) : base(context)
        {
            _antSettings = antSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Shoot);

        protected override bool Filter(AntEntity entity)
            => entity.isShoot && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.skeletonAnimation.Value.AnimationName = "attack";

                Observable.Timer(TimeSpan.FromSeconds(_antSettings.archerShootDelay))
                    .Subscribe(_ => Attack(entity))
                    .AddTo(_disposable);
                Observable.Timer(TimeSpan.FromSeconds(_antSettings.archerIdleDelay))
                    .Subscribe(_ =>
                    {
                        if (!entity.isDead && entity.hasSkeletonAnimation)
                            entity.skeletonAnimation.Value.AnimationName = "breath";
                    })
                    .AddTo(_disposable);
            }
        }

        private static void Attack(AntEntity entity)
        {
            if (entity.isDead)
                return;
            if (!entity.hasArcherView)
                return;
            
            var archerView = entity.archerView.Value;
            var mb = entity.mbUnit.Value;

            var arrow = Pools.Instance
                .Get(PoolType.ArrowFight, HackRefsView.Instance.FightView.ParentArrows)
                .GetComponent<ArrowView>();
            arrow.transform.position = mb.ArrowPos.position;
            arrow.IsEnabled = true;
            arrow.Y = arrow.transform.localPosition.y;
            archerView.Arrows.Add(arrow);

            entity.ReplaceAttackTime(0f);
            entity.isShoot = false;
        }

        public void Dispose() => _disposable?.Dispose();
    }
}