using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class InitChestModelsCommand : Command
    {
        public InitChestModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<ChestModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<ChestStatModel>>());
        }
    }
}