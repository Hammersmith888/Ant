using System.Collections.Generic;
using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using Entitas;

namespace Ecs.Sources.Battle.Systems
{
    public class LogoSystem : ReactiveSystem<BattleEntity>
    {
        private readonly BattleContext _battle;
        private readonly BattleSettingsInstaller.BattleSettings battleSettings;

        public LogoSystem(BattleContext battle,
            BattleSettingsInstaller.BattleSettings battleSettings) : base(battle)
        {
            _battle = battle;
            this.battleSettings = battleSettings;
        }

        protected override ICollector<BattleEntity> GetTrigger(IContext<BattleEntity> context)
            => context.CreateCollector(BattleMatcher.FightState);

        protected override bool Filter(BattleEntity entity)
            => entity.hasFightState && entity.fightState.Value == EFightState.Logo;

        protected override void Execute(List<BattleEntity> entities)
        {
            var currentRoom = _battle.currentRoomEntity;
            var battleRoom = currentRoom.battleRoom.Value;
            
            battleRoom.Stock.gameObject.SetActive(true);
            battleRoom.LevelBG.SetActive(false);
            BattleRooms.Instance.AnimateLogo();
            
            var chest = battleRoom.Chest;
            chest.CustomMaterialOverride.Remove(battleSettings.chestMaterialNormal);
        }
    }
}