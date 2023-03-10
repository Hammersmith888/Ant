using System;
using System.Threading.Tasks;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.TypeRegistry;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class CreateBuildingCommand : ICommand<CreateBuildingProtocol>
    {
        private readonly SceneEntityStorage _entityControllerStorage;
        private readonly BuildingModelStorage _buildingModelStorage;
        private readonly BuildingStatModelStorage _statModelStorage;
        private readonly TypeStorage _typeStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly IInstantiator _instantiator;
        private readonly StatsCollectionStorage _statsCollectionStorage;

        public CreateBuildingCommand(SceneEntityStorage entityControllerStorage,
                                     BuildingModelStorage buildingModelStorage,
                                     BuildingStatModelStorage statModelStorage,
                                     TypeStorage typeStorage,
                                     BuildingDtoStorage buildingDtoStorage,
                                     IInstantiator instantiator,
                                     StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _buildingModelStorage = buildingModelStorage;
            _statModelStorage = statModelStorage;
            _typeStorage = typeStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _entityControllerStorage = entityControllerStorage;
            _instantiator = instantiator;
        }

        public Task Execute(CreateBuildingProtocol protocol)
        {
            BuildingDto buildingDto;
            string controllerID;

            if (!string.IsNullOrEmpty(protocol.ModelID))
            {
                var model = _buildingModelStorage.Get(protocol.ModelID);
                controllerID = model.TypeName;
                buildingDto = new BuildingDto{Guid = protocol.Guid, ModelID =  protocol.ModelID, PlaceNum = protocol.PlaceNum};
                _buildingDtoStorage.Add(buildingDto);
            }
            else
            {
                buildingDto = _buildingDtoStorage.Get(protocol.Guid);
                var model = _buildingModelStorage.Get(buildingDto.ModelID);
                controllerID = model.TypeName;
            }

            
            if (!_statsCollectionStorage.HasEntity(buildingDto.Guid) && _statModelStorage.HasEntity(buildingDto.ModelID))
            {
                var stats = new StatModel[0];
                stats = _statModelStorage.Get(buildingDto.ModelID).Stats;
                var collectionProtocol = new CreateStatsCollectionProtocol(buildingDto.Guid, stats);
                _instantiator.Instantiate<CreateStatsCollectionCommand<BuildingStatsCollection>>().Execute(collectionProtocol);
            }
            
            if (!_typeStorage.HasEntity(controllerID))
            {
                throw new ArgumentException($"{this} : Building :: Controller Type with " +
                                            $"[ controllerID : {controllerID}] does not exist");
            }

            if (!_entityControllerStorage.HasEntity(buildingDto.Guid))
            {
                var concreteType = _typeStorage.Get(controllerID);
                var controller = (ISceneEntity)_instantiator.Instantiate(concreteType.Type, new object[] {buildingDto.Guid});
                _entityControllerStorage.Add(controller);
                
                var buildSceneObjectProtocol = new CreateBuildingSceneObjectProtocol(buildingDto.Guid);
                _instantiator.Instantiate<CreateBuildingSceneObjectCommand>().Execute(buildSceneObjectProtocol);

                var placeProtocol = new PlaceBuildingProtocol(buildingDto.ModelID, buildingDto.Guid, 
                    buildingDto.PlaceNum, protocol.InternalBuild);
                _instantiator.Instantiate<PlaceBuildingCommand>().Execute(placeProtocol);

                if (controller is IInitializable initializableController)
                {
                    initializableController.Initialize();
                }
            }


   

            return Task.CompletedTask;
        }
    }

    public struct CreateStatsProtocol : IProtocol
    {
        public string Guid;
        public string ModelID;
    }

    public class PostLoadAllBuildingsStatsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly BuildingStatModelStorage _buildingStatModelStorage;

        public PostLoadAllBuildingsStatsCommand(IInstantiator instantiator, 
            BuildingDtoStorage buildingDtoStorage,
            BuildingStatModelStorage buildingStatModelStorage)
        {
            _buildingStatModelStorage = buildingStatModelStorage;
            _instantiator = instantiator;
            _buildingDtoStorage = buildingDtoStorage;
        }

        public override void Do()
        {
            foreach (var buildingDto in _buildingDtoStorage.Get())
            {
                if(!_buildingStatModelStorage.HasEntity(buildingDto.ModelID))
                    continue;
                
                _instantiator.Instantiate<CreateBuildingStatsCommand>().Execute(new CreateStatsProtocol()
                {
                    Guid = buildingDto.Guid,
                    ModelID = buildingDto.ModelID
                });
            }
            OnDone();
        }
    }
    public class CreateBuildingStatsCommand : ICommand<CreateStatsProtocol>
    {
        private readonly BuildingStatModelStorage _statModelStorage;
        private readonly IInstantiator _instantiator;

        public CreateBuildingStatsCommand(BuildingStatModelStorage statModelStorage,
                                          IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _statModelStorage = statModelStorage;
        }

        public Task Execute(CreateStatsProtocol protocol)
        {
            var stats = new StatModel[0];
            if (_statModelStorage.HasEntity(protocol.ModelID))
            {
                stats = _statModelStorage.Get(protocol.ModelID).Stats;
            }
            var collectionProtocol = new CreateStatsCollectionProtocol(protocol.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<BuildingStatsCollection>>().Execute(collectionProtocol);
            return Task.CompletedTask;
        }
    }
}