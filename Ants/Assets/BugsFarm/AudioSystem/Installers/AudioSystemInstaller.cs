using System;
using Zenject;

namespace BugsFarm.AudioSystem
{
    public class AudioSystemInstaller : Installer<AudioSystemInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<AudioModelStorage>()
                .AsSingle();

            Container
                .Bind<SoundPlayer>()
                .FromComponentInNewPrefabResource("Audio/SoundPlayer")
                .AsSingle();

            Container
                .Bind<ISoundSystem>()
                .To<SoundSystem>()
                .AsSingle();

            Container
                .Bind(typeof(IMusicSystem), typeof(IInitializable), typeof(IDisposable))
                .To<MusicSystem>()
                .AsSingle();
        }
    }
}