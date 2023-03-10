using System.Threading.Tasks;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.Services.CommandService;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class CreateUnitSceneObjectCommand : ICommand<CreateUnitSceneObjectProtocol>
    {
        private readonly UnitModelStorage _modelsStorage;
        private readonly UnitSceneObjectStorage _unitViewStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitsContainer _parent;
        private readonly IInstantiator _instantiator;
        private readonly PrefabLoader _prefabLoader;
        
        public CreateUnitSceneObjectCommand(UnitModelStorage modelsStorage,
                                           UnitSceneObjectStorage unitViewStorage,
                                           UnitDtoStorage unitDtoStorage,
                                           UnitsContainer parent,
                                           IInstantiator instantiator,
                                           PrefabLoader prefabLoader)
        {     
            _modelsStorage = modelsStorage;
            _unitViewStorage = unitViewStorage;
            _unitDtoStorage = unitDtoStorage;
            _parent = parent;
            _instantiator = instantiator;
            _prefabLoader = prefabLoader;
        }
        
        public Task Execute(CreateUnitSceneObjectProtocol protocol)
        {
            var model  = _modelsStorage.Get(protocol.ModelId);
            var prefab = _prefabLoader.Load(model.TypeName);
            var args = new object[] {protocol.UnitId};
            var view = _instantiator.InstantiatePrefabForComponent<UnitSceneObject>(prefab, _parent.Transform, args);

            _unitViewStorage.Add(view);

            return Task.CompletedTask;
        }
    }
}