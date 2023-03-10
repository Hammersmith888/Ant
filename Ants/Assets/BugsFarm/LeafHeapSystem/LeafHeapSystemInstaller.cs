using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class LeafHeapSystemInstaller : Installer<LeafHeapSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<LeafHeapDtoStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<LeafHeapSceneObjectStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<LeafHeapModelStorage>()
                .AsSingle()
                .NonLazy();

            Container
                .BindInterfacesAndSelfTo<LeafHeapStatModelStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}