using BugsFarm.SimulationSystem;
using BugsFarm.UI;
using Zenject;

namespace BugsFarm.TaskSystem
{
    public class TaskTestInstaller : MonoInstaller<TaskTestInstaller>
    {
        public override void InstallBindings()
        {
            SimulationSystemInstaller.Install(Container);
            Container.BindInterfacesAndSelfTo<TaskTestRunner>().AsSingle().NonLazy();
        }

        public override void Start()
        {
            base.Start();
            Container.Instantiate<UISimulationInteractor>().Initialize();
        }
    }
}