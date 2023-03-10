using Zenject;

namespace BugsFarm.InventorySystem
{
    public class InventorySystemInstaller : Installer<InventorySystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<InventoryDtoStorage>()
                .AsSingle()
                .NonLazy();           
            
            Container
                .BindInterfacesAndSelfTo<InventoryStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .BindInterfacesAndSelfTo<InventoryItemModelStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}