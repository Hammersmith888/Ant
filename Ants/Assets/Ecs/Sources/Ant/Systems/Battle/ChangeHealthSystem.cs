using System.Collections.Generic;
using BugsFarm.Model.Enum;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle
{
    public class ChangeHealthSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleContext _battle;

        public ChangeHealthSystem(IContext<AntEntity> context,
            BattleContext battle) : base(context)
        {
            _battle = battle;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Health);

        protected override bool Filter(AntEntity entity)
            => _battle.fightState.Value == EFightState.Fight &&
               entity.hasHealth && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var mbUnit = entity.mbUnit.Value;

                if (entity.health.Value <= 0f)
                {
                    entity.isDead = true;
                    mbUnit.HpBar.gameObject.SetActive(false);
                }
                else
                {
                    var percent = entity.health.Value / entity.healthMax.Value;

                    if (!mbUnit.HpBar.gameObject.activeSelf)
                        mbUnit.HpBar.gameObject.SetActive(true);

                    mbUnit.SetHpBar(percent);
                }
            }
        }
    }
}