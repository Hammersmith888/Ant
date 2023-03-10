using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class CreateBuildingSceneObjectCommand : ICommand<CreateBuildingSceneObjectProtocol>
    {
        private readonly BuildingModelStorage _modelsStorage;
        private readonly BuildingDtoStorage _dtoStorage;
        private readonly BuildingSceneObjectStorage _viewStorage;
        private readonly BuildingsContainer _parent;
        private readonly IInstantiator _instantiator;
        private readonly PrefabLoader _prefabLoader;
        
        public CreateBuildingSceneObjectCommand(BuildingModelStorage modelsStorage,
                                                BuildingDtoStorage dtoStorage,
                                                BuildingSceneObjectStorage viewStorage,
                                                BuildingsContainer parent,
                                                IInstantiator instantiator,
                                                PrefabLoader prefabLoader)
        {
            _modelsStorage = modelsStorage;
            _dtoStorage = dtoStorage;
            _viewStorage = viewStorage;
            _parent = parent;
            _instantiator = instantiator;
            _prefabLoader = prefabLoader;
        }
        
        public Task Execute(CreateBuildingSceneObjectProtocol protocol)
        {
            if(_viewStorage.HasEntity(protocol.Guid))
                return Task.CompletedTask;
            
            var dto    = _dtoStorage.Get(protocol.Guid);
            var model  = _modelsStorage.Get(dto.ModelID);
            var prefab = _prefabLoader.Load(model.TypeName);
            var view   = _instantiator.InstantiatePrefabForComponent<BuildingSceneObject>(prefab, _parent.Transform, new []{protocol.Guid});
            _viewStorage.Add(view);
            
            return Task.CompletedTask;
        }
    }
}