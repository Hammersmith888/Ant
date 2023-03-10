using BugsFarm.Services.BootstrapService;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class PostLoadUnitsCommand : Command
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;

        public PostLoadUnitsCommand(UnitDtoStorage unitDtoStorage, 
                                    IInstantiator instantiator)
        {
            _unitDtoStorage = unitDtoStorage;
            _instantiator = instantiator;
        }
        public override void Do()
        {
            var unitBuildingCommand = _instantiator.Instantiate<CreateUnitCommand>();
            foreach (var unitDto in _unitDtoStorage.Get())
            {
                var unitBuildProtocol = new CreateUnitProtocol(unitDto.Guid, false);
                unitBuildingCommand.Execute(unitBuildProtocol);
            }
            
            OnDone();
        }
    }
}