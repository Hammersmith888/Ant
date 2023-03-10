using Zenject;

namespace BugsFarm.Locations
{
    public class LocationsInstaller : Installer<LocationsInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<LocationSceneObject>()
                .FromComponentInNewPrefabResource("Locations/Default")
                .AsSingle()
                .NonLazy();
        }
    }
}