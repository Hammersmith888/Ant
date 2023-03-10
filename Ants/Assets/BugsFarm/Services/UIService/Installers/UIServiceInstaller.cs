using Zenject;

namespace BugsFarm.Services.UIService
{
    public class UIServiceInstaller : Installer<UIServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<UIRoot>()
                .FromComponentInNewPrefabResource("UIWindows/UIRoot")
                .AsSingle()
                .NonLazy();

            Container
                .Bind<UIWorldRoot>()
                .FromComponentInNewPrefabResource("UIWindows/UIWorldRoot")
                .AsSingle();

            Container
                .Bind<IUIService>()
                .To<UIService>()
                .AsSingle()
                .NonLazy();
        }
    }
}