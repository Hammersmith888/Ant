using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.Quest
{
    public class InitQuestModelsCommand : Command
    {
        public InitQuestModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<QuestElementModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<QuestGroupModel>>());
        }
    }
}