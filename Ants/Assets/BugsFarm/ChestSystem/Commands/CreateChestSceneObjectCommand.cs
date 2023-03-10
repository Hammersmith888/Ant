using System;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.CommandService;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.ChestSystem
{
    public class CreateChestSceneObjectCommand : ICommand<CreateChestSceneObjectPrtotocol>
    {
        private readonly PrefabLoader _prefabLoader;
        private readonly RoomsContainer _container;
        private readonly IInstantiator _instantiator;
        private readonly ChestModelStorage _chsetModelStorage;
        private readonly ChestDtoStorage _dtoStorage;
        private readonly ChestSceneObjectStorage _viewStorage;
        private const string _prefabNameKey = "Chest_";
        private const string _prefabTypeNameKey = "ChestType_";
        public CreateChestSceneObjectCommand(PrefabLoader prefabLoader,
                                            RoomsContainer container,
                                            IInstantiator instantiator,
                                            ChestModelStorage  chsetModelStorage,
                                            ChestDtoStorage dtoStorage,
                                            ChestSceneObjectStorage viewStorage)
        {
            _prefabLoader = prefabLoader;
            _container = container;
            _instantiator = instantiator;
            _chsetModelStorage = chsetModelStorage;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
        }

        public Task Execute(CreateChestSceneObjectPrtotocol protocol)
        {
            if (!_dtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(Execute)} :: {nameof(ChestDto)} with [Guid : {protocol.Guid}], does not exist.");
            }

            if (_viewStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Execute)} :: {nameof(ChestSceneObject)} with [Guid : {protocol.Guid}], alredy exist.");
            }

            var dto = _dtoStorage.Get(protocol.Guid);
            var model = _chsetModelStorage.Get(dto.ModelID);
            var parentPrefab = _prefabLoader.Load(_prefabNameKey + dto.ModelID);
            
            var prefabTypeChest = _prefabLoader.Load(_prefabTypeNameKey + model.TypeID);
            var parent = _instantiator.InstantiatePrefab(parentPrefab, _container.Transform);
            var sceneObject = _instantiator.InstantiatePrefabForComponent<ChestSceneObject>(prefabTypeChest, _container.Transform, new object[]{protocol.Guid});
            _viewStorage.Add(sceneObject);

            var position = sceneObject.transform.position;
            position = parent.transform.position;
            position = new Vector3(position.x, position.y, -0.8f);
            sceneObject.transform.position = position;

            Object.Destroy(parent);

            return Task.CompletedTask;
        }
    }
}