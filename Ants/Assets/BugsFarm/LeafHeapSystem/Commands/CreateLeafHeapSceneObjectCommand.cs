using System;
using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.LeafHeapSystem
{
    public class CreateLeafHeapSceneObjectCommand : ICommand<CreateLeafHeapSceneObjectProtocol>
    {
        private readonly PrefabLoader _prefabLoader;
        private readonly RoomsContainer _container;
        private readonly IInstantiator _instantiator;
        private readonly LeafHeapDtoStorage _dtoStorage;
        private readonly LeafHeapSceneObjectStorage _viewStorage;
        private const string _preafabNameKey = "LeafHeap_";
        public CreateLeafHeapSceneObjectCommand(PrefabLoader prefabLoader,
                                                 RoomsContainer container,
                                                 IInstantiator instantiator,
                                                 LeafHeapDtoStorage dtoStorage,
                                                 LeafHeapSceneObjectStorage viewStorage)
        {
            _prefabLoader = prefabLoader;
            _container = container;
            _instantiator = instantiator;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
        }
        
        public Task Execute(CreateLeafHeapSceneObjectProtocol protocol)
        {
            if (!_dtoStorage.HasEntity(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(Execute)} :: {nameof(LeafHeapDto)} with [Guid : {protocol.Guid}], does not exist.");
            }
            if (_viewStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{this} : {nameof(Execute)} :: {nameof(LeafHeapSceneObject)} with [Guid : {protocol.Guid}], alredy exist.");
            }

            var dto         = _dtoStorage.Get(protocol.Guid);
            var prefabName  = _preafabNameKey + dto.ModelID;
            var prefab      = _prefabLoader.Load(prefabName);
            var sceneObject = _instantiator.InstantiatePrefabForComponent<LeafHeapSceneObject>(prefab, _container.Transform);
            sceneObject.Id = protocol.Guid;
            _viewStorage.Add(sceneObject);
            
            return Task.CompletedTask;
        }
    }
}