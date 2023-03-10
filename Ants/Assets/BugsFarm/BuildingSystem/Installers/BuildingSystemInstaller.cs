using BugsFarm.BuildingSystem.DeathSystem;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingSystemInstaller : Installer<BuildingSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<BuildingsContainer>()
                .FromComponentInNewPrefabResource("Prefabs/Buildings/BuildingsContainer")
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<BuildingShopItemModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingDtoStorage>()
                .AsSingle()
                .NonLazy();
            Container
                .BindInterfacesAndSelfTo<BuildingModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingOpenableModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingParamsModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingStageModelStorage>()
                .AsSingle();
                            
            Container
                .BindInterfacesAndSelfTo<BuildingStatModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingUpgradeModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingRestockModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingSpeedupModelStorage>()
                .AsSingle();
                            
            Container
                .BindInterfacesAndSelfTo<BuildingSceneObjectStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingInfoParamsModelsStorage>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<OrderModelsStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<OrderUsedStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<OrderDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<AntHillTaskDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<AntHillTaskModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<AntHillTaskHandler>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<HospitalSlotModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<HospitalSlotDtoStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<BuildingDeathSystem>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<ResurrectBuildingRegistrySystem>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<ResurrectBuildingDataStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<PrisonSlotModelStorage>()
                .AsSingle();

            BuildingBuildSystemInstaller.Install(Container);
            BuildingPlaceSystemInstaller.Install(Container);
            BuildingResourceSystemInstaller.Install(Container);
            
            Container
                .BindInterfacesAndSelfTo<BuildingOpenableSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}