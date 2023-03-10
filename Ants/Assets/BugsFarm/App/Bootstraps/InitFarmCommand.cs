using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.AudioSystem;
using BugsFarm.BattleSystem;
using BugsFarm.BootstrapCommon;
using BugsFarm.BuildingSystem;
using BugsFarm.ChestSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.FarmCameraSystem;
using BugsFarm.InventorySystem;
using BugsFarm.LeafHeapSystem;
using BugsFarm.Quest;
using BugsFarm.RoomSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulatingSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.App
{
    public class InitFarmCommand : Command
    {
        public InitFarmCommand(string contextName, IBootstrap bootstrap)
        {
            var args = new object[] {bootstrap};
            var context = GameObject.Find(contextName).GetComponent<SceneContext>();
            var instantiator = (IInstantiator)context.Container; // context container!
            // Main System
            bootstrap.AddCommand(instantiator.Instantiate<InitStatsCommand>());
            bootstrap.AddCommand(instantiator.Instantiate<FarmCameraCalibrationCommand>());
            bootstrap.AddCommand(instantiator.Instantiate<InitAudioSystemCommand>());
            bootstrap.AddCommand(instantiator.Instantiate<InitAstarGraphCommand>());

            // Other Systems
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BattlePassModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<DonatShopItemModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<MyBugItemModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<CurrencySettingModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<PhrasesModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<InventoryItemModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingsSimulationGroup>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<SimulatingFoodOrderModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitsSimulationGroupModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<SimulatingRoomGroupModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<SimulatingTrainingModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<SimulatingUnitAssignableTaskModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<SimulatingUnitSleepModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitAnimationModelsCommand>());
            
            // Game systems
            bootstrap.AddCommand(instantiator.Instantiate<InitRoomModelsCommand>(args));
            bootstrap.AddCommand(instantiator.Instantiate<InitQuestModelsCommand>(args));
            bootstrap.AddCommand(instantiator.Instantiate<InitLeafHeapsModelsCommand>(args));
            bootstrap.AddCommand(instantiator.Instantiate<InitChestModelsCommand>(args));
            bootstrap.AddCommand(instantiator.Instantiate<InitBuildingModelsCommand>(args));
            bootstrap.AddCommand(instantiator.Instantiate<InitUnitModelsCommand>(args));

            // Load or First play init
            bootstrap.AddCommand(instantiator.Instantiate<LoadOrFirstPlayCommand>(args));
            
            // Post init
            bootstrap.AddCommand(instantiator.Instantiate<InitOldSystemsCommand>());
            bootstrap.AddCommand(instantiator.Instantiate<UISafeAreaCalibrationCommand>());
            bootstrap.AddCommand(instantiator.Instantiate<UIFarmInitCommand>());
        }
    }
}