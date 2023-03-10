using Zenject;

namespace BugsFarm.Services.TypeRegistry
{
    public class TypeRegistryInstaller : Installer<TypeRegistryInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<TypeStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}