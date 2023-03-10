using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UnityEngine;

namespace BugsFarm.LeafHeapSystem
{
    public class DeleteLeafHeapCommand : ICommand<DeleteLeafHeapProtocol>
    {
        private readonly SceneEntityStorage _entityController;
        private readonly LeafHeapDtoStorage _dtoStorage;
        private readonly LeafHeapSceneObjectStorage _viewStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        public DeleteLeafHeapCommand(SceneEntityStorage entityController,
                                     LeafHeapDtoStorage dtoStorage,
                                     LeafHeapSceneObjectStorage viewStorage,
                                     StatsCollectionStorage statsCollectionStorage)
        {
            _entityController = entityController;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }

        public Task Execute(DeleteLeafHeapProtocol protocol)
        {
            if (_entityController.HasEntity(protocol.Guid))
            {
                var entity = _entityController.Get(protocol.Guid);
                _entityController.Remove(protocol.Guid);
                entity.Dispose();
            }

            if (_dtoStorage.HasEntity(protocol.Guid))
            {
                _dtoStorage.Remove(protocol.Guid);
            }

            if (_statsCollectionStorage.HasEntity(protocol.Guid))
            {
                _statsCollectionStorage.Remove(protocol.Guid);
            }

            if (_viewStorage.HasEntity(protocol.Guid))
            {
                var view = _viewStorage.Get(protocol.Guid);
                _viewStorage.Remove(protocol.Guid);
                Object.Destroy(view.gameObject);
            }

            return Task.CompletedTask;
        }
    }
}