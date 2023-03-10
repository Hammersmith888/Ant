using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using Entitas;

namespace Ecs.Sources.Battle.Systems
{
    public class TurnLightOnSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly AntContext _ant;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly BattleService _battleService;

        public TurnLightOnSystem(
            BattleContext battle,
            AntContext ant,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleService battleService) : base(battle)
        {
            _battle = battle;
            _ant = ant;
            _battleSettings = battleSettings;
            _battleService = battleService;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.TurnLightOn;

        protected override void Execute(List<BattleEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            
            BattleService.TweenDarkness(
                battleRoom,
                0,
                _battleSettings.lightOnDuration,
                _battleSettings.lightOnDelay);
            
            EnableEnemies();

            var chest = _ant.GetEntityWithChestRoomIndex(currentRoom.roomIndex.Value);
            chest.AddFadeIn(0f);

            _battle.ReplaceFightState(EFightState.ExitFromCave);
        }

        private void EnableEnemies()
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            var enemyGroup = _ant.GetGroup(AntMatcher.AllOf(AntMatcher.Enemy, AntMatcher.Disabled));

            foreach (var entity in enemyGroup.GetEntities())
            {
                if (entity.room.Value == battleRoom)
                {
                    entity.isPatrol = false;
                    entity.isDisabled = false;
                }
            }
        }
    }
}