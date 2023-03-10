using BugsFarm.AssetLoaderSystem;
using BugsFarm.AudioSystem;
using BugsFarm.CurrencySystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.InputService;
using BugsFarm.Services.InteractorSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StateMachine;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.TypeRegistry;
using BugsFarm.Services.UIService;
using BugsFarm.UserSystem;
using Zenject;

namespace BugsFarm.App
{
    public class AppInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //Services
            StatsServiceInstaller.Install(Container);
            InteractorServiceInstaller.Install(Container);
            StateMachineInstaller.Install(Container);
            SaveManagerInstaller.Install(Container);
            TypeRegistryInstaller.Install(Container);
            BootstrapInstaller.Install(Container);
            AssetLoaderInstaller.Install(Container);
            InputServiceInstaller.Install(Container);
            UIServiceInstaller.Install(Container);
            CurrencySystemInstaller.Install(Container);
            UsersIntaller.Install(Container);
            AudioSystemInstaller.Install(Container);
            MonoPoolInstaller.Install(Container);
            Container.Bind<AppInstaller>().FromInstance(this).AsSingle();
        }

        public override void Start()
        {
            base.Start();
            Container.Instantiate<GameLoader>().Init();
        }
    }
}