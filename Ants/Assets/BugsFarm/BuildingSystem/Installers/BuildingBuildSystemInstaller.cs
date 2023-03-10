using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class BuildingBuildSystemInstaller : Installer<BuildingBuildSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<BuildingBuildSystem>()
                .AsSingle();
        }
    }
}