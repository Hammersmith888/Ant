using Zenject;

namespace BugsFarm.SpeakerSystem
{
    public class SpeakerSystemInstaller : Installer<SpeakerSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<PhrasesStorage>()
                .AsSingle()
                .NonLazy();             
            
            Container
                .Bind<ISpeakerSystem>()
                .To<SpeakerSystem>()
                .AsSingle()
                .NonLazy();
        }
    }
}