using BugsFarm.Installers;
using BugsFarm.Services;
using BugsFarm.Views.Fight;
using BugsFarm.Views.Screen;
using Ecs.Sources.Ant.Systems.Battle;
using Ecs.Sources.Ant.Systems.Battle.Attack;
using Ecs.Sources.Ant.Systems.Battle.Booster;
using Ecs.Sources.Ant.Systems.Battle.Dead;
using Ecs.Sources.Ant.Systems.Battle.ExitFromCave;
using Ecs.Sources.Ant.Systems.Battle.Move;
using Ecs.Sources.Ant.Systems.Battle.Patrol;
using Ecs.Sources.Ant.Systems.Battle.Retreat;
using Ecs.Sources.Ant.Systems.Battle.Select;
using Ecs.Sources.Ant.Systems.Battle.Target;
using Ecs.Sources.Battle.Systems;
using Ecs.Sources.Resource.Systems;
using Entitas;
using Zenject;

namespace Ecs.Controllers
{
    public class BattleEcsController : IInitializable, ITickable
    {
        private readonly BattleSettingsInstaller.AntSettings _antSettings;
        private readonly AntService _antService;
        private readonly BattleService _battleService;
        private readonly ResourceService _resourceService;
        private readonly FightScreen _fightScreen;
        private readonly BattleSettingsInstaller.BattleSettings _battleSettings;
        private readonly BattleRooms _battleRooms;
        private readonly FightView _fightView;
        private readonly UnitViewSettingsInstaller.UnitViewSettings _unitViewSettings;
        private readonly UnitFightSettingsInstaller.UnitFightSettings _unitFightSettings;

        private Systems _systems;

        public BattleEcsController(
            BattleSettingsInstaller.AntSettings antSettings,
            AntService antService,
            BattleService battleService,
            ResourceService resourceService,
            FightScreen fightScreen,
            BattleSettingsInstaller.BattleSettings battleSettings,
            BattleRooms battleRooms,
            FightView fightView,
            UnitViewSettingsInstaller.UnitViewSettings unitViewSettings,
            UnitFightSettingsInstaller.UnitFightSettings unitFightSettings
        )
        {
            _antSettings = antSettings;
            _antService = antService;
            _battleService = battleService;
            _resourceService = resourceService;
            _fightScreen = fightScreen;
            _battleSettings = battleSettings;
            _battleRooms = battleRooms;
            _fightView = fightView;
            _unitViewSettings = unitViewSettings;
            _unitFightSettings = unitFightSettings;
        }

        private Systems InitSystems(Contexts context)
        {
            return new Feature("Systems")

                    // Battle
                    .Add(new InitBattleSystem(_battleService, _resourceService, _antService,
                        context.battle, _fightScreen, context.ant, _unitViewSettings))
                    .Add(new SpawnRoomSystem(context.battle, _battleRooms, _antService, _battleSettings,
                        context.ant))
                    .Add(new CameraMovementSystem(context.battle, _battleSettings))
                    .Add(new FightSystem(context.battle, _fightView))
                    .Add(new SquadSelectSystem(context.battle, _battleService, _fightView))
                    .Add(new SpawnSystem(_antService, context.ant, context.battle, _battleService, _fightView))
                    .Add(new TurnLightOnSystem(context.battle, context.ant, _battleSettings, _battleService))
                    .Add(new ExitFromCaveSystem(context.ant, context.battle, _battleSettings))
                    .Add(new LogoSystem(context.battle, _battleSettings))
                    .Add(new EnterCaveSystem(context.ant, context.battle))
                    .Add(new CameraTweenSystem(context.ant, _antSettings, context.battle, _battleSettings,
                        _battleService, _antService))
                    .Add(new NoneStateSystem(context.battle, _battleService))

                    // Ant Battle
                    .Add(new StartFightSystem(context.battle, context.ant, _antSettings, _battleService))
                    // Patrol
                    .Add(new PatrolAddedSystem(context.ant, _antService, _battleSettings))
                    .Add(new PatrolWaitSystem(context.ant))
                    .Add(new PatrolRemovedSystem(context.ant, _antService, _battleSettings))
                    // Target
                    .Add(new TargetAddedMeleeSystem(context.ant, _antService))
                    .Add(new TargetAddedRangedSystem(context.ant, _antService, _antSettings, _unitViewSettings))
                    .Add(new FindAttackPositionSystem(context.ant, _antSettings, _battleService))
                    // Move
                    .Add(new MoveToStartPositionAdded(context.ant, context.battle, _unitViewSettings))
                    .Add(new AntMoveSystem(context.ant, _antSettings, _antService, context.battle, _unitViewSettings))
                    .Add(new MoveAddedSystem(context.ant, _antSettings, _unitViewSettings))
                    .Add(new MoveRemovedSystem(context.ant, context.battle, _battleSettings, _battleService,
                        _unitViewSettings))
                    // Attack
                    .Add(new AttackAddedSystem(context.ant, _antSettings, _antService, _unitViewSettings))
                    .Add(new AttackRemovedSystem(context.ant, _unitViewSettings))
                    .Add(new AttackTimeSystem(context.ant, _antSettings, _antService, _battleSettings,
                        _unitViewSettings, _unitFightSettings))
                    .Add(new ShootSystem(context.ant, _antSettings))
                    .Add(new UpdateArrowsSystem(context.ant, _antSettings))
                    .Add(new DamageSystem(context.ant, _battleSettings, _fightView, _antSettings, _fightScreen))
                    .Add(new ChangeHealthSystem(context.ant, context.battle))
                    // Dead
                    .Add(new PlayerDeadSystem(context.ant, _fightScreen, _battleSettings, context.battle,
                        _battleService, _antSettings, _unitViewSettings))
                    .Add(new EnemyDeadSystem(context.ant, _fightScreen, _battleSettings, context.battle, _battleService,
                        _antSettings))
                    // Select
                    .Add(new InputSelectAntSystem(context.battle, context.ant, _antSettings))
                    .Add(new SelectAddedSystem(context.ant, _antSettings, _antService))
                    .Add(new SelectRemovedSystem(context.ant, _antSettings, _antService))
                    // Booster
                    .Add(new BoosterArmorAddedSystem(context.ant, _antSettings, _antService, _fightScreen))
                    .Add(new BoosterArmorRemovedSystem(context.ant, _antSettings, _antService, _fightScreen))
                    .Add(new BoosterAttackAddedSystem(context.ant, _antSettings, _antService, _fightScreen))
                    .Add(new BoosterAttackRemovedSystem(context.ant, _antSettings, _antService, _fightScreen))
                    .Add(new ArmorTimeSystem(context.battle, _battleSettings, context.ant))
                    .Add(new AttackBoostTimeSystem(context.battle, _battleSettings, context.ant))
                    // Other
                    .Add(new RetreatSystem(context.ant, context.battle))
                    .Add(new ExitFromCaveAddedSystem(context.ant, context.battle, _antSettings))
                    .Add(new ExitFromCaveRemovedSystem(context.ant, context.battle, _antSettings, _unitViewSettings))
                    .Add(new ExitFromCaveTimeSystem(context.ant, context.battle))
                    .Add(new EnterToCaveAddedSystem(context.ant, context.battle))
                    .Add(new FadeInTimerSystem(context.ant, _antSettings, _antService))

                    // Resources
                    .Add(new InitResourcesSystem(context.resource))
                ;
        }

        public void Initialize()
        {
            _systems = InitSystems(Contexts.sharedInstance);
            _systems.Initialize();
        }

        public void Tick()
        {
            _systems.Execute();
            _systems.Cleanup();
        }
    }
}