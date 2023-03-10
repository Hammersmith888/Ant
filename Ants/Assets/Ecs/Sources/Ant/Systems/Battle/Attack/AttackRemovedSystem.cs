using System.Collections.Generic;
using System.Linq;
using BugsFarm.Installers;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Attack
{
    public class AttackRemovedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public AttackRemovedSystem(AntContext ant,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(ant)
        {
            _ant = ant;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.AttackMelee.Removed());

        protected override bool Filter(AntEntity entity)
            => !entity.isAttackMelee && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.hasTarget)
                {
                    var target = _ant.GetEntityWithUid(entity.target.Uid);
                    if (!target.hasAttackers)
                        return;

                    var attackerUids = target.attackers.Uids.ToList();

                    if (attackerUids.Contains(entity.uid.Value))
                    {
                        attackerUids.Remove(entity.uid.Value);
                        target.ReplaceAttackers(attackerUids.ToArray());
                    }
                }

                var config = _unitViewSettings.units[entity.antType.Value];
                entity.skeletonAnimation.Value.timeScale = config.animationScale.idle;
            }
        }
    }
}