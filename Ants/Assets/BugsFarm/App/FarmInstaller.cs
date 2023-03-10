using BugsFarm.AnimationsSystem;
using BugsFarm.AstarGraph;
using BugsFarm.BattleSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.ChestSystem;
using BugsFarm.CurrencyCollectorSystem;
using BugsFarm.FarmCameraSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.LeafHeapSystem;
using BugsFarm.Locations;
using BugsFarm.Quest;
using BugsFarm.RoomSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.SimulatingSystem;
using BugsFarm.SimulationSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.App
{
    public class FarmInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            SimulationSystemInstaller.Install(Container);
            SimulatingSystemInstaller.Install(Container);
            LocationsInstaller.Install(Container);
            AstarGraphInstaller.Install(Container);
            UnitSystemInstaller.Install(Container);
            FarmCameraInstaller.Install(Container);
            BattleSystemInstaller.Install(Container);
            QuestInstaller.Install(Container);
            SceneEntityInstaller.Install(Container);
            AnimationSystemInstaller.Install(Container);
            RoomSystemInstaller.Install(Container);
            BuildingSystemInstaller.Install(Container);
            InventorySystemInstaller.Install(Container);
            TaskSystemInstaller.Install(Container);

            InfoCollectorInstaller.Install(Container);
            SpeakerSystemInstaller.Install(Container);
            LeafHeapSystemInstaller.Install(Container);
            ChestSystemInstaller.Install(Container);
            CurrencyCollectorSystemInstaller.Install(Container);
            UIBottomHudInstaller.Install(Container);

            Container
                .Bind<StatUIConfig>()
                .FromScriptableObjectResource("UIWindows/StatUIConfig")
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<AppState>()
                .AsSingle()
                .NonLazy();
        }
    }
}