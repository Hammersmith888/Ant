using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Select
{
    public class SelectAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;

        public SelectAddedSystem(IContext<AntEntity> context,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService) : base(context)
        {
            _antSettings = antSettings;
            _antService = antService;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Selected.Added());

        protected override bool Filter(AntEntity entity)
            => entity.isAnt && entity.isSelected && entity.isPlayer;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var antVo = _antService.GetAntVo(entity.antType.Value);
                if (!entity.skeletonAnimation.Value.CustomMaterialOverride.ContainsKey(antVo.materialOriginal))
                {
                    entity.skeletonAnimation.Value.CustomMaterialOverride.Add(
                        antVo.materialOriginal,
                        antVo.materialOutline);
                }
            }
        }
    }
}