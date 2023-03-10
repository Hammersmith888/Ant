using Zenject;

namespace BugsFarm.Services.SceneEntity
{
    public class SceneEntityInstaller : Installer<SceneEntityInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<SceneEntityStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}