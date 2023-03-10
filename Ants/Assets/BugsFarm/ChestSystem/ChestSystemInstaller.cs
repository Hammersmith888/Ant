using Zenject;

namespace BugsFarm.ChestSystem
{
    public class ChestSystemInstaller : Installer<ChestSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<ChestDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<ChestModelStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<ChestSceneObjectStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<ChestStatModelStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<ChestOpenableSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}