using BugsFarm.Utility;
using Zenject;

namespace BugsFarm.AssetLoaderSystem
{
    public class AssetLoaderInstaller : Installer<AssetLoaderInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<IconLoader>()
                .AsSingle();

            Container
                .Bind<PrefabLoader>()
                .AsSingle();
            
            Container
                .Bind<SpriteStageLoader>()
                .AsSingle();
            
            Container
                .Bind<SpineLoader>()
                .AsSingle();
            
            Container
                .Bind<SoundLoader>()
                .AsSingle();
        }
    }
}