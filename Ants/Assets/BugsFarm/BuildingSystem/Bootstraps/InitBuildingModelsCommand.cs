using BugsFarm.BootstrapCommon;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.TypeRegistry;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class InitBuildingModelsCommand : Command
    {
        private readonly IInstantiator _instantiator;

        public InitBuildingModelsCommand(IInstantiator instantiator, IBootstrap bootstrap)
        {
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingOpenablesModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingParamModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingShopItemModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingStageModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingStatModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingUpgradeModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingInfoParamsModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingRestockModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<BuildingSpeedupModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<OrderModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<HospitalSlotModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<PrisonSlotModel>>());
            bootstrap.AddCommand(instantiator.Instantiate<InitModelsCommand<AntHillTaskModel>>());
        }
    }
}