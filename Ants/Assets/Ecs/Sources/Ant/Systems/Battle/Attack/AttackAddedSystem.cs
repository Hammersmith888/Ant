using System.Collections.Generic;
using System.Linq;
using BugsFarm.Installers;
using BugsFarm.Services;
using Ecs.Utils;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Attack
{
    public class AttackAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public AttackAddedSystem(AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(ant)
        {
            _ant = ant;
            _antSettings = antSettings;
            _antService = antService;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.AttackMelee.Added());

        protected override bool Filter(AntEntity entity)
            => entity.isAttackMelee && entity.hasTarget && !entity.isDead;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                entity.isWaitOpponent = false;

                var config = _unitViewSettings.units[entity.antType.Value];
                var antVo = _antService.GetAntVo(entity.antType.Value);

                entity.ReplaceAttackTime(config.attackDelay - 0.25f);
                entity.ReplaceAttackTimeMax(config.attackDelay);

                var target = _ant.GetEntityWithUid(entity.target.Uid);
                var attackerUids = new List<Uid>();
                if (target.hasAttackers)
                    attackerUids = target.attackers.Uids.ToList();
                if (!attackerUids.Contains(entity.uid.Value))
                    attackerUids.Add(entity.uid.Value);

                target.ReplaceAttackers(attackerUids.ToArray());
                CheckFindAttackPosition(entity);

                entity.skeletonAnimation.Value.timeScale = antVo.attackAnimationScale;
            }
        }

        private void CheckFindAttackPosition(AntEntity entity)
        {
            var targetGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player, AntMatcher.Target)
                .NoneOf(AntMatcher.Dead, AntMatcher.AttackMelee, AntMatcher.RangedUnit));

            foreach (var groupEntity in targetGroup.GetEntities())
            {
                if (groupEntity.target.Uid == entity.uid.Value)
                    groupEntity.isFindAttackPosition = true;
            }
        }
    }
}