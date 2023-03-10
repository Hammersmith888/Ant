using BugsFarm.BuildingSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class UnitSystemInstaller : Installer<UnitSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<UnitsContainer>()
                .FromComponentInNewPrefabResource("Prefabs/Units/UnitsContainer")
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<UnitBirthModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<UnitCivilRegistryDtoStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<UnitDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<TaskDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<UnitModelStorage>()
                .AsSingle()
                .NonLazy();
                
            Container
                .BindInterfacesAndSelfTo<UnitMoverStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<UnitMoverDtoStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<UnitShopItemsStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<UnitSceneObjectStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<UnitStatModelStorage>()
                .AsSingle()
                .NonLazy();   

            Container
                .BindInterfacesAndSelfTo<UnitStageModelStorage>()
                .AsSingle()
                .NonLazy();     
                
            Container
                .BindInterfacesAndSelfTo<UnitTaskProcessorStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<UnitTaskModelStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<UnitUpgradeModelStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<PathHelper>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<TaskBuilderSystem>()
                .AsSingle();

            Container
                .Bind<IActivitySystem>()
                .To<ActivitySystem>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<UnitCivilRegistrySystem>()
                .AsSingle(); 
            
            Container
                .BindInterfacesAndSelfTo<UnitDeathSystem>()
                .AsSingle(); 
            
            Container
                .BindInterfacesAndSelfTo<UnitDrinkSystem>()
                .AsSingle(); 
            
            Container
                .BindInterfacesAndSelfTo<UnitEatSystem>()
                .AsSingle(); 
            
            Container
                .BindInterfacesAndSelfTo<UnitSleepSystem>()
                .AsSingle();
            
            Container
                .Bind<IUnitFallSystem>()
                .To<UnitFallSystem>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<LarvaPointDtoStorage>()
                .AsSingle();
        }
    }
}