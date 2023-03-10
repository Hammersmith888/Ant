using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using Entitas;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Ecs.Sources.Ant.Systems.Battle
{
    public class StartFightSystem : ReactiveSystem<BattleEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly BattleService _battleService;

        public StartFightSystem(BattleContext battle,
            AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            BattleService battleService
        ) : base(battle)
        {
            _ant = ant;
            _antSettings = antSettings;
            _battleService = battleService;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.Fight;

        protected override void Execute(List<BattleEntity> entities)
        {
            var playerGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Player)
                .NoneOf(AntMatcher.Disabled, AntMatcher.Dead, AntMatcher.Target));
            var enemyGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy)
                .NoneOf(AntMatcher.Disabled, AntMatcher.Dead, AntMatcher.Target));

            foreach (var entity in playerGroup.GetEntities())
                _battleService.SetNextTarget(entity);

            foreach (var entity in enemyGroup.GetEntities())
                _battleService.SetNextTarget(entity);
        }
    }
}