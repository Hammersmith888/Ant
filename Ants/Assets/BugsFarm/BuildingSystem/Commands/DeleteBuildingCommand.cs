using System.Threading.Tasks;
using BugsFarm.AudioSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.TaskSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class DeleteBuildingCommand : ICommand<DeleteBuildingProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly ISoundSystem _soundSystem;
        private readonly TaskStorage _taskStorage;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly BuildingSceneObjectStorage _sceneObjectStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly InventoryStorage _inventoryStorage;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly SceneEntityStorage _entityControllerStorage;
        private readonly IReservedPlaceSystem _reservedPlaceSystem;

        public DeleteBuildingCommand(IInstantiator instantiator,
                                     ISoundSystem soundSystem,
                                     TaskStorage taskStorage,
                                     AudioModelStorage audioModelStorage,
                                     BuildingSceneObjectStorage sceneObjectStorage,
                                     BuildingDtoStorage buildingDtoStorage,
                                     StatsCollectionStorage statsCollectionStorage,
                                     InventoryStorage inventoryStorage,
                                     InventoryDtoStorage inventoryDtoStorage,
                                     SceneEntityStorage entityControllerStorage,
                                     IReservedPlaceSystem reservedPlaceSystem)
        {
            _instantiator = instantiator;
            _soundSystem = soundSystem;
            _taskStorage = taskStorage;
            _audioModelStorage = audioModelStorage;
            _sceneObjectStorage = sceneObjectStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _inventoryStorage = inventoryStorage;
            _inventoryDtoStorage = inventoryDtoStorage;
            _entityControllerStorage = entityControllerStorage;
            _reservedPlaceSystem = reservedPlaceSystem;
        }

        public Task Execute(DeleteBuildingProtocol protocol)
        {
            var guid = protocol.Guid;
            var dto = _buildingDtoStorage.Get(guid);

            _instantiator.Instantiate<DeleteEntityHandler>(new object[] {protocol.Guid});
            // удаление существующих задач
            if (_taskStorage.HasTasks(guid))
            {
                _taskStorage.RemoveAll(guid);
            }

            // удаление котроллера
            if (_entityControllerStorage.HasEntity(guid))
            {
                var controller = _entityControllerStorage.Get(guid);
                _entityControllerStorage.Remove(guid);
                controller.Dispose();
            }
            
            // удаление DTO
            if (_buildingDtoStorage.HasEntity(guid))
            {
                _buildingDtoStorage.Remove(guid);
            }
            
            // удаление статов
            if (_statsCollectionStorage.HasEntity(guid))
            {
                _statsCollectionStorage.Remove(guid);
            }

            // удаление занятых мест
            if (_reservedPlaceSystem.HasEntity(dto.PlaceNum))
            {
                _reservedPlaceSystem.Remove(dto.PlaceNum, protocol.Notify);
            }
            
            // удаление инвентаря
            if (_inventoryStorage.HasEntity(guid))
            {
                _inventoryStorage.Remove(guid);
            }
            
            // удаление данных инвентаря
            if (_inventoryDtoStorage.HasEntity(guid))
            {
                _inventoryDtoStorage.Remove(guid);
            }
            
            // удаление вьюхи
            if (_sceneObjectStorage.HasEntity(guid))
            {
                var view = _sceneObjectStorage.Get(guid);
                _sceneObjectStorage.Remove(guid);
                Object.Destroy(view.gameObject);
            }
            
            // звуковое сопровождение
            if(protocol.Notify)
            {
                _soundSystem.Play(GetAudioClip(dto.ModelID));
            }

            MessageBroker.Default.Publish(protocol);
            return Task.CompletedTask;
        }
        
        private string GetAudioClip(string modelID)
        {
            var audioModel = _audioModelStorage.Get("Buildings");
            return audioModel.GetAudioClip("Remove");
        }
    }
}