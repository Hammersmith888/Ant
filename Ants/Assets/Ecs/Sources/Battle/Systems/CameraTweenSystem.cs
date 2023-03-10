using System;
using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using Entitas;
using UniRx;
using UnityEngine;

namespace Ecs.Sources.Battle.Systems
{
    public class CameraTweenSystem : ReactiveSystem<BattleEntity>
    {
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly BattleService _battleService;
        private readonly AntService _antService;
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        public CameraTweenSystem(
            AntContext ant,
            BattleSettingsInstaller.AntSettings antSettings,
            BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleService battleService,
            AntService antService) : base(battle)
        {
            _ant = ant;
            _antSettings = antSettings;
            _battle = battle;
            _battleSettings = battleSettings;
            _battleService = battleService;
            _antService = antService;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.CameraTween;

        protected override void Execute(List<BattleEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            BattleService.TweenDarkness(battleRoom, 1, _battleSettings.lightOffDuration, 0);

            var nextRoom = _battle.GetEntityWithRoomIndex(currentRoom.roomIndex.Value + 1);
            currentRoom.isCurrentRoom = false;
            nextRoom.isCurrentRoom = true;

            if (_battle.season.Value == 2)
            {
                var nextBattleRoom = nextRoom.battleRoom.Value;
                nextBattleRoom.Glass.GetComponent<Animator>().enabled = true;
            }

            var enemyDeadGroup = _ant.GetGroup(AntMatcher
                .AllOf(AntMatcher.Enemy, AntMatcher.Dead)
                .NoneOf(AntMatcher.Disabled));
            foreach (var entity in enemyDeadGroup.GetEntities())
            {
                var antVo = _antService.GetAntVo(entity.antType.Value);
                
                entity.skeletonAnimation.Value.CustomMaterialOverride.Add(antVo.materialOriginal, antVo.materialFill);
                entity.isDisabled = true;
            }

            _battleService.MoveCamera();

            Observable.Timer(TimeSpan.FromSeconds(_battleSettings.cameraMoveDelay))
                .Subscribe(_ => _battle.ReplaceFightState(EFightState.TurnLightOn))
                .AddTo(_disposable);

            _battle.isSpawnRoom = true;
        }
    }
}