using Zenject;

namespace BugsFarm.Services.InputService
{
    public class InputServiceInstaller : Installer<InputServiceInstaller>
    {
        public override void InstallBindings()
        {
            Container
                .BindInterfacesAndSelfTo<InputController<SceneLayer>>()
                .AsSingle();

            Container
                .BindInterfacesAndSelfTo<MainInputController>()
                .AsSingle();
                
            Container
                .BindInterfacesAndSelfTo<HybridInputSystem>()
                .AsSingle();
            
            Container
                .BindInterfacesAndSelfTo<SwipeSystem>()
                .AsSingle();
        }
    }
}