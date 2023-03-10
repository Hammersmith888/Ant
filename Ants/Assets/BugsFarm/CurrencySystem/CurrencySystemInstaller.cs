using Zenject;

namespace BugsFarm.CurrencySystem
{
    public class CurrencySystemInstaller : Installer<CurrencySystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<CurrencySettingStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}