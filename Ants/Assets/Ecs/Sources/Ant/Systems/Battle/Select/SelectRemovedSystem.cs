using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Select
{
    public class SelectRemovedSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;

        public SelectRemovedSystem(IContext<AntEntity> context,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService) : base(context)
        {
            _antSettings = antSettings;
            _antService = antService;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Selected.Removed());

        protected override bool Filter(AntEntity entity)
            => entity.isAnt && !entity.isSelected;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var antVo = _antService.GetAntVo(entity.antType.Value);
                entity.skeletonAnimation.Value.CustomMaterialOverride.Remove(antVo.materialOriginal);
            }
        }
    }
}