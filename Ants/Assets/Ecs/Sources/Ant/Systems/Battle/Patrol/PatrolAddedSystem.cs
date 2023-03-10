using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Services;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Systems.Battle.Patrol
{
    public class PatrolAddedSystem : ReactiveSystem<AntEntity>
    {
        private readonly AntService _antService;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;

        public PatrolAddedSystem(IContext<AntEntity> context,
            AntService antService,
            BattleSettingsInstaller.BattleSettings battleSettings) : base(context)
        {
            _antService = antService;
            _battleSettings = battleSettings;
        }

        protected override ICollector<AntEntity> GetTrigger(IContext<AntEntity> context)
            => context.CreateCollector(AntMatcher.Patrol.Added());

        protected override bool Filter(AntEntity entity)
            => entity.isPatrol;

        protected override void Execute(List<AntEntity> entities)
        {
            foreach (var entity in entities)
            {
                var transform = entity.transform.Value;
                var random = Random.Range(0f, 1f);
                var antVo = _antService.GetAntVo(entity.antType.Value);
                var destination = transform.position;

                if (random < .5f)
                    destination.x -= antVo.patrolDistance;
                else
                    destination.x += antVo.patrolDistance;

                destination.x = Mathf.Clamp(destination.x, _battleSettings.patrolBorderLeft,
                    _battleSettings.patrolBorderRight);
                
                entity.AddMoveToPosition(destination);
            }
        }
    }
}