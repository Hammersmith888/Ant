using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingResourceSystemInstaller : Installer<BuildingResourceSystemInstaller>
    {
        public override void InstallBindings()
        {

            Container
                .BindInterfacesAndSelfTo<GetResourceSystem>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<AddResourceSystem>()
                .AsSingle();
        }
    }
}