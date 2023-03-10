using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class InitFirstPlayChestsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly ChestModelStorage _chestModelsStorage;

        public InitFirstPlayChestsCommand(IInstantiator instantiator,
                                          ChestModelStorage chestModelsStorage)
        {
            _instantiator = instantiator;
            _chestModelsStorage = chestModelsStorage;
        }

        public override void Do()
        {
            var buildChestCommand = _instantiator.Instantiate<CreateChestCommand>();
            foreach (var model in _chestModelsStorage.Get())
            {
                var buildChestProcotol = new CreateChestProtocol(model.ModelID, true);
                buildChestCommand.Execute(buildChestProcotol);
            }

            OnDone();
        }
    }
}