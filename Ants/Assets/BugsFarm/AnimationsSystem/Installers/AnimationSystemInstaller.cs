using Zenject;

namespace BugsFarm.AnimationsSystem
{
    public class AnimationSystemInstaller : Installer<AnimationSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .Bind<AnimationModelStorage>()
                .AsSingle()
                .NonLazy(); 
                
            Container
                .Bind<AnimatorStorage>()
                .AsSingle()
                .NonLazy();
        }
    }
}