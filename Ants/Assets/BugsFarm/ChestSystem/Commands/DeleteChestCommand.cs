using System.Threading.Tasks;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UnityEngine;

namespace BugsFarm.ChestSystem
{
    public class DeleteChestCommand : ICommand<DeleteChestProtocol>
    {
        private readonly SceneEntityStorage _entityController;
        private readonly ChestDtoStorage _chestDtoStorage;
        private readonly ChestSceneObjectStorage _viewStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        public DeleteChestCommand(SceneEntityStorage entityController,
                                  ChestDtoStorage chestDtoStorage,
                                  ChestSceneObjectStorage viewStorage,
                                  StatsCollectionStorage statsCollectionStorage)
        {
            _entityController = entityController;
            _chestDtoStorage = chestDtoStorage;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
        }
        
        public Task Execute(DeleteChestProtocol protocol)
        {
            if (_entityController.HasEntity(protocol.Guid))
            {
                var entity = _entityController.Get(protocol.Guid);
                _entityController.Remove(protocol.Guid);
                entity.Dispose();
            }
            
            if (_chestDtoStorage.HasEntity(protocol.Guid))
            {
                _chestDtoStorage.Remove(protocol.Guid);
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