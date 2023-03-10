using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class InitUnitModelsCommand : Command
    {
        public InitUnitModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitBirthModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitShopItemModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitStageModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitStatModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitTasksModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<UnitUpgradeModel>>());
        }
    }
}