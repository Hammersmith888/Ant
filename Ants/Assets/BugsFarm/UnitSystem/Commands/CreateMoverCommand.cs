using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class CreateMoverCommand<T> : ICommand<CreateMoverProtocol> where T : IUnitMover
    {
        private readonly IInstantiator _instantiator;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitMoverStorage _moverStorage;
        private readonly UnitMoverDtoStorage _moverDtoStorage;

        public CreateMoverCommand(IInstantiator instantiator,
                                  UnitDtoStorage unitDtoStorage,
                                  UnitMoverStorage moverStorage,
                                  UnitMoverDtoStorage moverDtoStorage)
        {
            _instantiator = instantiator;
            _unitDtoStorage = unitDtoStorage;
            _moverStorage = moverStorage;
            _moverDtoStorage = moverDtoStorage;
        }

        public Task Execute(CreateMoverProtocol protocol)
        {
            if (!_unitDtoStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException("UnitDto does not exist");
            }
            var unitDto = _unitDtoStorage.Get(protocol.Guid);
            
            if (!_moverDtoStorage.HasEntity(protocol.Guid))
            {
                _moverDtoStorage.Add(new UnitMoverDto{Guid = protocol.Guid, ModelID = unitDto.ModelID});
            }
            
            if (!_moverStorage.HasEntity(protocol.Guid))
            {
                var args = protocol.Args.Prepend(_moverDtoStorage.Get(protocol.Guid));
                var unitMover = _instantiator.Instantiate<T>(args);
                _moverStorage.Add(unitMover);
                unitMover.Initialize();
                protocol.Result?.Invoke(unitMover);
            }
            
            return Task.CompletedTask;
        }
    }
}