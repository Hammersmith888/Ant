using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class DeleteUnitCommand : ICommand<DeleteUnitProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly UnitDtoStorage _dtoStorage;

        public DeleteUnitCommand(IInstantiator instantiator,
                                 SceneEntityStorage sceneEntityController,
                                 UnitDtoStorage dtoStorage)
        {
            _instantiator = instantiator;
            _sceneEntityController = sceneEntityController;
            _dtoStorage = dtoStorage;
        }

        public Task Execute(DeleteUnitProtocol protocol)
        {
            if (_sceneEntityController.HasEntity(protocol.Guid))
            {
                var unit = _sceneEntityController.Get(protocol.Guid);
                _sceneEntityController.Remove(protocol.Guid);
                unit.Dispose();
            }

            if (_dtoStorage.HasEntity(protocol.Guid))
            {
                _dtoStorage.Remove(protocol.Guid);
            }

            _instantiator.Instantiate<DeleteStatsCollectionCommand>()
                .Execute(new DeleteStatsCollectionProtocol(){Guid = protocol.Guid});
            _instantiator.Instantiate<DeleteUnitSceneObjectCommand>()
                .Execute(new DeleteUnitSceneObjectProtocol(protocol.Guid));
            
            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
    }
}