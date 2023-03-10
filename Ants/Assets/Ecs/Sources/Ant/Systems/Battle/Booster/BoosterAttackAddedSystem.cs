using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using BugsFarm.Views.Screen;
using Entitas;

namespace Ecs.Sources.Ant.Systems.Battle.Booster
{
    public class BoosterAttackAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly FightScreen _fightScreen;

        public BoosterAttackAddedSystem(IContext<AntEntity> context,
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService,
            FightScreen fightScreen) : base(context)
        {
            _antSettings = antSettings;
            _antService = antService;
            _fightScreen = fightScreen;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.AttackBoost.Added());

        protected override bool Filter(AntEntity entity)
            => entity.isAnt && entity.isAttackBoost && entity.isPlayer;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var antVo = _antService.GetAntVo(entity.antType.Value);
                entity.skeletonAnimation.Value.CustomMaterialOverride.Add(
                    antVo.materialOriginal,
                    antVo.materialAttack);

                _fightScreen.AttackButton.EnabledImage.enabled = true;
                _fightScreen.ArmorButton.Button.interactable = false;
            }
        }
    }
}