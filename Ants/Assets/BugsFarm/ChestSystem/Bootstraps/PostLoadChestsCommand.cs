using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.ChestSystem
{
    public class PostLoadChestsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly ChestDtoStorage _chestDtoStorage;
        public PostLoadChestsCommand(IInstantiator instantiator, 
                                     ChestDtoStorage chestDtoStorage)
        {
            _instantiator = instantiator;
            _chestDtoStorage = chestDtoStorage;
        }
        public override void Do()
        {
            var buildingCommand = _instantiator.Instantiate<CreateChestCommand>();
            foreach (var chestDto in _chestDtoStorage.Get())
            {
                buildingCommand.Execute(new CreateChestProtocol(chestDto.Guid,false));
            }
            OnDone();
        }
    }
}