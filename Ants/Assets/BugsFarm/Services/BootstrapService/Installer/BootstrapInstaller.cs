using BugsFarm.ReloadSystem;
using Zenject;

namespace BugsFarm.Services.BootstrapService
{
    public class BootstrapInstaller : Installer<BootstrapInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IBootstrap>()
                .To<Bootstrap>()
                .AsTransient();
            
        }
    }
}