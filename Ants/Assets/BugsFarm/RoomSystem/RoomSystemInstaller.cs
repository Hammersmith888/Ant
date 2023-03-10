using Zenject;

namespace BugsFarm.RoomSystem
{
    public class RoomSystemInstaller : Installer<RoomSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<RoomsContainer>()
                .FromNewComponentOnNewGameObject()
                .WithGameObjectName("RoomsContainer")
                .AsSingle();
            
            // Storages
            Container
                .BindInterfacesAndSelfTo<RoomDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<RoomModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<RoomNeighboursModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<RoomStatModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<RoomSceneObjectStorage>()
                .AsSingle()
                .NonLazy();
            
            
            // Systems
            Container
                .Bind<IRoomsSystem>()
                .To<RoomsSystem>()
                .AsSingle();
        }
    }
}