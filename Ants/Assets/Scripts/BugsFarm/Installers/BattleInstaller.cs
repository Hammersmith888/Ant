using BugsFarm.Services;
using Ecs.Controllers;
using Zenject;

namespace BugsFarm.Installers
{
    public class BattleInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            //Container.BindInterfacesAndSelfTo<ScreenService>().AsSingle();

            InstallEcs();
            InstallServices();
        }

        private void InstallEcs()
        {
            Container.BindInterfacesTo<BattleEcsController>().AsSingle();

            var contexts = Contexts.sharedInstance;
            Container.BindInstance(contexts.game);
            Container.BindInstance(contexts.input);
            Container.BindInstance(contexts.ant);
            Container.BindInstance(contexts.resource);
            Container.BindInstance(contexts.battle);
        }

        private void InstallServices()
        {
            Container.BindInterfacesAndSelfTo<BattleService>().AsSingle();
            Container.BindInterfacesAndSelfTo<ResourceService>().AsSingle();
            Container.BindInterfacesAndSelfTo<AntService>().AsSingle();
        }
    }
}