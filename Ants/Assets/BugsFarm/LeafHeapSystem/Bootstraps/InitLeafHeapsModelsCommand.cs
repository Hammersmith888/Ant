using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class InitLeafHeapsModelsCommand : Command
    {
        public InitLeafHeapsModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<LeafHeapModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<LeafHeapStatModel>>());
        }
    }
}