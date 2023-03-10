using Zenject;

namespace BugsFarm.Services.InteractorSystem
{
    public class InteractorServiceInstaller : Installer<InteractorServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<InteractorStorage>()
                .AsSingle();
        }
    }
}