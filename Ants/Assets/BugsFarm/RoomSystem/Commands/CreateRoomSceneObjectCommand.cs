using System;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class CreateRoomSceneObjectCommand : ICommand<CreateRoomSceneObjectProtocol>
    {
        private readonly PrefabLoader _prefabLoader;
        private readonly RoomsContainer _container;
        private readonly IInstantiator _instantiator;
        private readonly RoomDtoStorage _dtoStorage;
        private readonly RoomModelStorage _modelsStorage;
        private readonly RoomSceneObjectStorage _viewStorage;

        private const string _roomNameKey = "Room_";

        public CreateRoomSceneObjectCommand(PrefabLoader prefabLoader,
                                             RoomsContainer container,
                                             IInstantiator instantiator,
                                             RoomDtoStorage dtoStorage,
                                             RoomSceneObjectStorage viewStorage)
        {
            _prefabLoader = prefabLoader;
            _container = container;
            _instantiator = instantiator;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
        }

        public Task Execute(CreateRoomSceneObjectProtocol protocol)
        {
            if (!_dtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(Execute)} :: {nameof(RoomDto)} with [Guid : {protocol.Guid}], does not exist.");
            }
            
            if (_viewStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Execute)} :: {nameof(RoomBaseSceneObject)} with [Guid : {protocol.Guid}], alredy exist.");
            }

            var dto = _dtoStorage.Get(protocol.Guid);
            var prefabName = _roomNameKey + dto.ModelID;
            var prefab = _prefabLoader.Load(prefabName);
            var sceneObject = _instantiator.InstantiatePrefabForComponent<RoomBaseSceneObject>(prefab,_container.Transform, new object[]{protocol.Guid});
            _viewStorage.Add(sceneObject);

            return Task.CompletedTask;
        }
    }
}