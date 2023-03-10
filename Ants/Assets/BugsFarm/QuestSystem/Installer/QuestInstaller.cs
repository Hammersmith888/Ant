using Zenject;

namespace BugsFarm.Quest
{
    public class QuestInstaller : Installer<QuestInstaller>
    {
        public override void InstallBindings()
        {
            Container.BindInterfacesAndSelfTo<QuestElementModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestElementDtoStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestGroupModelStorage>().AsSingle().NonLazy();
            Container.BindInterfacesAndSelfTo<QuestGroupDtoStorage>().AsSingle().NonLazy();
            
            Container.Bind<QuestSystemController>().AsSingle().NonLazy();
        }
    }
}