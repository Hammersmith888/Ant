using Zenject;

namespace BugsFarm.UI
{
    public class UIBottomHudInstaller: Installer<UIBottomHudInstaller>

    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<DonatShopItemModelStorage>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<MyBugsItemModelStorage>()
                .AsSingle();
        }
    }
}