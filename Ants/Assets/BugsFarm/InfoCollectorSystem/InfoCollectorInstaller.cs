using Zenject;

namespace BugsFarm.InfoCollectorSystem
{
    public class InfoCollectorInstaller : Installer<InfoCollectorInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<StateInfoStorage>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<ResourceInfoStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<ResourceBarInfoStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<BuildingInfoStorage>()
                .AsSingle();
        }
    }
}