using System.Collections.Generic;
using BugsFarm.Installers;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.ExitFromCave
{
    public class ExitFromCaveRemovedSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public ExitFromCaveRemovedSystem(IContext<AntEntity> context,
            BattleContext battle,
            BattleSettingsInstaller.AntSettings antSettings,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings) : base(context)
        {
            _battle = battle;
            _antSettings = antSettings;
            _unitViewSettings = unitViewSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.ExitFromCave.Removed());

        protected override bool Filter(AntEntity entity)
            => !entity.isExitFromCave && entity.hasSkeletonAnimation;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var config = _unitViewSettings.units[entity.antType.Value];
                entity.skeletonAnimation.Value.timeScale = config.animationScale.idle;
            }
        }
    }
}