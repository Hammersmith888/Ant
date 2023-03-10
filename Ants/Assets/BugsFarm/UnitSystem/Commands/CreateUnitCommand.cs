using System;
using System.Linq;
using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.TypeRegistry;
using BugsFarm.Utility;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class CreateUnitCommand : ICommand<CreateUnitProtocol>
    {
        private readonly TypeStorage _typeStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitStatModelStorage _unitStatModelStorage;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IInstantiator _instantiator;

        public CreateUnitCommand(TypeStorage typeStorage,
                                   UnitDtoStorage unitDtoStorage,
                                   UnitModelStorage unitModelStorage,
                                   UnitStatModelStorage unitStatModelStorage,
                                   SceneEntityStorage sceneEntityController,
                                   IInstantiator instantiator,
                                   StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _typeStorage = typeStorage;
            _unitDtoStorage = unitDtoStorage;
            _unitModelStorage = unitModelStorage;
            _unitStatModelStorage = unitStatModelStorage;
            _sceneEntityController = sceneEntityController;
            _instantiator = instantiator;
        }

        public Task Execute(CreateUnitProtocol protocol)
        {
            UnitDto unitDto;
            string controllerID;

            if (!string.IsNullOrEmpty(protocol.ModelID))
            {
                var model = _unitModelStorage.Get(protocol.ModelID);
                var nameID = UnitsUtils.GenerateNameKey(model.IsFemale);
                controllerID = model.TypeName;
                unitDto = new UnitDto(protocol.Guid, protocol.ModelID, nameID);
                _unitDtoStorage.Add(unitDto);
            }
            else
            {
                unitDto = _unitDtoStorage.Get(protocol.Guid);
                var model = _unitModelStorage.Get(unitDto.ModelID);
                controllerID = model.TypeName;
            }

            if (!_statsCollectionStorage.HasEntity(unitDto.Guid))
            {
                var stats = new StatModel[0];
                stats = _unitStatModelStorage.Get(unitDto.ModelID).Stats;
                var collectionProtocol = new CreateStatsCollectionProtocol(unitDto.Guid, stats);
                _instantiator.Instantiate<CreateStatsCollectionCommand<UnitStatsCollection>>().Execute(collectionProtocol);
            }
            
            if (!_typeStorage.HasEntity(controllerID))
            {
                throw new InvalidOperationException($"{this} : ControllerID : {controllerID}, does not exist");
            }

            var concreteType = _typeStorage.Get(controllerID);
            var args = protocol.Args.Prepend(protocol.Guid);
            var controller = (ISceneEntity) _instantiator.Instantiate(concreteType.Type, args);
            _sceneEntityController.Add(controller);

            var sceneObjectProtocol = new CreateUnitSceneObjectProtocol(unitDto.Guid, unitDto.ModelID);
            _instantiator.Instantiate<CreateUnitSceneObjectCommand>().Execute(sceneObjectProtocol);

            if (controller is IInitializable initializable)
            {
                initializable.Initialize();
            }

            return Task.CompletedTask;
        }
    }

    public class PostLoadAllUnitsStatsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly UnitDtoStorage _unitDtoStorage;

        public PostLoadAllUnitsStatsCommand(IInstantiator instantiator, UnitDtoStorage unitDtoStorage)
        {
            _instantiator = instantiator;
            _unitDtoStorage = unitDtoStorage;
        }

        public override void Do()
        {
            foreach (var unitDto in _unitDtoStorage.Get())
            {
                _instantiator.Instantiate<CreateUnitStatsCommand>().Execute(new CreateStatsProtocol()
                {
                    Guid = unitDto.Guid,
                    ModelID = unitDto.ModelID 
                });
            }
            
            OnDone();
        }
    }


    public class CreateUnitStatsCommand : ICommand<CreateStatsProtocol>
    {
        private readonly IInstantiator _instantiator;
        private readonly UnitStatModelStorage _unitStatModelStorage;

        public CreateUnitStatsCommand(UnitStatModelStorage unitStatModelStorage,
                                      IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _unitStatModelStorage = unitStatModelStorage;
        }

        public Task Execute(CreateStatsProtocol protocol)
        {
            var stats = new StatModel[0];
            if (_unitStatModelStorage.HasEntity(protocol.ModelID))
            {
                stats = _unitStatModelStorage.Get(protocol.ModelID).Stats;
            }
            var collectionProtocol = new CreateStatsCollectionProtocol(protocol.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<UnitStatsCollection>>().Execute(collectionProtocol);
            return Task.CompletedTask;
        }
    }
}