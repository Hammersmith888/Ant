using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Patrol
{
    public class PatrolRemovedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntService _antService;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;

        public PatrolRemovedSystem(IContext<AntEntity> context,
            AntService antService,
            BattleSettingsInstaller.BattleSettings battleSettings) : base(context)
        {
            _antService = antService;
            _battleSettings = battleSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Patrol.Removed());

        protected override bool Filter(AntEntity entity)
            => !entity.isPatrol;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                if (entity.hasWaitPatrol)
                    entity.RemoveWaitPatrol();
            }
        }
    }
}