using Zenject;

namespace BugsFarm.Services.SaveManagerService
{
    public class SaveManagerInstaller : Installer<SaveManagerInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<ISavableStorage>()
                .To<SavableStorage>()
                .AsSingle()
                .NonLazy();
            
            Container
                .Bind<ISerializeHelper>()
                .To<SerializeHelper>()
                .AsSingle();
            
            Container
                .Bind<ISaveManager>()
                .To<SaveManager>()
                .AsSingle();
            
            Container
                .Bind<ISaveManagerLocal>()
                .To<SaveManagerLocal>()
                .AsSingle();
        }
    }
}