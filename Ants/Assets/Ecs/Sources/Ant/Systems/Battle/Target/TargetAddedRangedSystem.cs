using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Target
{
    public class TargetAddedRangedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntService _antService;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public TargetAddedRangedSystem(AntContext ant,
            AntService antService,
            BattleSettingsInstaller.AntSettings antSettings,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(ant)
        {
            _antService = antService;
            _antSettings = antSettings;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Target.Added());

        protected override bool Filter(AntEntity entity)
            => entity.hasTarget && entity.antType.Value == AntType.Archer;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var config = _unitViewSettings.units[entity.antType.Value];
                var attackTime = config.attackDelay;

                attackTime -= Random.Range(_antSettings.archerShootTimeNoise.x, _antSettings.archerShootTimeNoise.y);
                entity.ReplaceAttackTime(attackTime);
                entity.ReplaceAttackTimeMax(config.attackDelay);
            }
        }
    }
}