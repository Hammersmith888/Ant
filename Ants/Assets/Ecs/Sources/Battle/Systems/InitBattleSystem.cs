using BugsFarm.Installers;
using BugsFarm.Model.Enum;
using BugsFarm.Services;
using BugsFarm.Views.Screen;
using Entitas;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Ecs.Sources.Battle.Systems
{
    public class InitBattleSystem : IInitializeSystem
    {
        private readonly BattleService _battleService;
        private readonly ResourceService _resourceService;
        private readonly AntService _antService;
        private readonly BattleContext _battle;
        private readonly FightScreen _fightScreen;
        private readonly AntContext _ant;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;

        public InitBattleSystem(
            BattleService battleService,
            ResourceService resourceService,
            AntService antService,
            BattleContext battle,
            FightScreen fightScreen,
            AntContext ant,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings)
        {
            _battleService = battleService;
            _resourceService = resourceService;
            _antService = antService;
            _battle = battle;
            _fightScreen = fightScreen;
            _ant = ant;
            _unitViewSettings = unitViewSettings;
        }

        public void Initialize()
        {
            Screen.sleepTimeout = SleepTimeout.NeverSleep;
            
            var etalonAspect = 1080f / 1920;
            if (Camera.main.aspect < etalonAspect)
                Camera.main.orthographicSize *= etalonAspect / Camera.main.aspect;
            else
                _fightScreen.CanvasScaler.matchWidthOrHeight = 1;

            // todo debug
            _battle.ReplaceSeason(SceneManager.GetActiveScene().name == "Battle Season 1" ? 1 : 2);
            _resourceService.UpdateResource(EResourceType.FoodStock, 100);

            _battle.ReplaceLastRoomIndex(_unitViewSettings.startRoom);
            _battle.ReplaceFightState(EFightState.None);

            _battle.isSpawnRoom = true;
            _battle.ReplaceFightState(EFightState.Spawn);
        }
    }
}