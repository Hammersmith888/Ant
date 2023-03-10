using Zenject;

namespace BugsFarm.TaskSystem
{
    public class TaskSystemInstaller : Installer<TaskSystemInstaller>
    {
        public override void InstallBindings()
        {

            Container
                .Bind<TaskStorage>()
                .AsSingle()
                .NonLazy();

        }
    }
}