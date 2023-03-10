using Zenject;

namespace BugsFarm.BattleSystem
{
    public class BattleSystemInstaller : Installer<BattleSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<BattlePassModelsStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}