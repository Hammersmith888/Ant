using BugsFarm.ReloadSystem;
using BugsFarm.SimulatingSystem.AssignableTasks;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingSystemInstaller : Installer<SimulatingSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container.Bind<SimulatingCenter>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingProcess>().AsSingle();

            Container.Bind<SimulatingMudStockCreator>().AsSingle().NonLazy();
            Container.Bind<SimulatingAntLarvaCreator>().AsSingle().NonLazy();
            Container.Bind<SimulatingUnitsKiller>().AsSingle();
            Container.Bind<SimulatingTeleporter>().AsSingle();
            Container.Bind<SimulatingTaskAssigner>().AsSingle();
            
            Container.Bind<CheckSafeFullnessSimulatingStage>().AsSingle();
            Container.Bind<ExcludeUnbuildedBuildingsSimulatingStage>().AsSingle();
            Container.Bind<UpdateGardensSimulationStage>().AsSingle();
            Container.Bind<FeedFoodBugsSimulatingStage>().AsSingle();
            Container.Bind<FeedWaterBugsSimulatingStage>().AsSingle();
            Container.Bind<OpenRoomsSimulatingStage>().AsSingle();
            Container.Bind<BirthLarvaSimulatingStage>().AsSingle();
            Container.Bind<DailyQuestSimulatingStage>().AsSingle();
            Container.Bind<GrowLarvaSimulatingStage>().AsSingle();
            Container.Bind<TrainingSimulatingStage>().AsSingle();
            Container.Bind<WorkingSimulatingStage>().AsSingle();
            Container.Bind<UpdateOrdersSimulatingStage>().AsSingle();
            Container.Bind<UnitsRepositionSimulatingStage>().AsSingle();
            Container.Bind<UnitsAssignTaskSimulationStage>().AsSingle();

            Container.Bind<GameReloader>().AsSingle();

            Container.BindInterfacesAndSelfTo<SimulatingEntityStorage>().AsSingle();
            Container.BindInterfacesAndSelfTo<BuildingSimulationGroupStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingFoodOrderModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<UnitsSimulationGroupModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingRoomGroupModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingTrainingModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingUnitAssignableTaskModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<SimulatingUnitSleepModelStorage>().AsSingle().NonLazy();

        }
    }
}