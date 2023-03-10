using System.Threading.Tasks;
using BugsFarm.Services.CommandService;

namespace BugsFarm.UnitSystem
{
    public class DeleteMoverCommand : ICommand<DeleteMoverProtocol>
    {
        private readonly UnitMoverStorage _moverStorage;
        private readonly UnitMoverDtoStorage _moverDtoStorage;

        public DeleteMoverCommand(UnitMoverStorage moverStorage,
                                  UnitMoverDtoStorage moverDtoStorage)
        {
            _moverStorage = moverStorage;
            _moverDtoStorage = moverDtoStorage;
        }

        public Task Execute(DeleteMoverProtocol protocol)
        {
            if (_moverStorage.HasEntity(protocol.Guid))
            {
                var unitMover = _moverStorage.Get(protocol.Guid);
                _moverStorage.Remove(protocol.Guid);
                unitMover.Dispose();
            }
            
            if (_moverDtoStorage.HasEntity(protocol.Guid))
            {
                _moverDtoStorage.Remove(protocol.Guid);
            }
            
            return Task.CompletedTask;
        }
    }
}