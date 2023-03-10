using Zenject;

namespace BugsFarm.CurrencyCollectorSystem
{
    public class CurrencyCollectorSystemInstaller : Installer<CurrencyCollectorSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<ICurrencyAnimation>()
                .To<CurrencyCollectorAnimation>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<CurrencyCollectorSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}