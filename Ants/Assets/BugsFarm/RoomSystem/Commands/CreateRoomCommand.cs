using System;
using System.Threading.Tasks;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.TypeRegistry;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class CreateRoomCommand : ICommand<CreateRoomProtocol>
    {
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IInstantiator _instantiator;
        private readonly SceneEntityStorage _sceneEntityController;
        private readonly RoomDtoStorage _dtoStorage;
        private readonly RoomModelStorage _modelStorage;
        private readonly RoomStatModelStorage _statModelStorage;
        private readonly TypeStorage _typeStorage;

        public CreateRoomCommand(IInstantiator instantiator,
                                SceneEntityStorage sceneEntityController,
                                RoomDtoStorage dtoStorage,
                                RoomModelStorage modelStorage,
                                RoomStatModelStorage statModelStorage,
                                TypeStorage typeStorage,
                                StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _instantiator = instantiator;
            _sceneEntityController = sceneEntityController;
            _dtoStorage = dtoStorage;
            _modelStorage = modelStorage;
            _statModelStorage = statModelStorage;
            _typeStorage = typeStorage;
        }

        public Task Execute(CreateRoomProtocol protocol)
        {
            RoomDto roomDto;
            string controllerID;

            if (!string.IsNullOrEmpty(protocol.ModelID))
            {
                var model = _modelStorage.Get(protocol.ModelID);
                controllerID = model.TypeName;
                roomDto = new RoomDto(protocol.ModelID, protocol.Guid);
                _dtoStorage.Add(roomDto);
            }
            else
            {
                roomDto = _dtoStorage.Get(protocol.Guid);
                var model = _modelStorage.Get(roomDto.ModelID);
                controllerID = model.TypeName;
            }

            if (!_statsCollectionStorage.HasEntity(roomDto.Guid))
            {
                var stats = new StatModel[0];
                stats = _statModelStorage.Get(roomDto.ModelID).Stats;
                var collectionProtocol = new CreateStatsCollectionProtocol(roomDto.Guid, stats);
                _instantiator.Instantiate<CreateStatsCollectionCommand<StatsCollection>>().Execute(collectionProtocol);
            }

            
            if (!_typeStorage.HasEntity(controllerID))
            {
                throw new
                    InvalidOperationException($"{this} : {nameof(Execute)} :: Controller with [TypeName : {controllerID}], does not exist.");
            }
            
            var concreteType = _typeStorage.Get(controllerID);
            var entity = (ISceneEntity) _instantiator.Instantiate(concreteType.Type, new[] {protocol.Guid});
            _sceneEntityController.Add(entity);

            var buildSceneObjectProtocol = new CreateRoomSceneObjectProtocol(protocol.Guid);
            _instantiator.Instantiate<CreateRoomSceneObjectCommand>().Execute(buildSceneObjectProtocol);

            if (entity is IInitializable initializable)
            {
                initializable.Initialize();
            }

            return Task.CompletedTask;
        }
    }

    public class PostLoadAllRoomsStatsCommand : Command
    {
        private readonly IInstantiator _instantiator;
        private readonly RoomDtoStorage _roomDtoStorage;

        public PostLoadAllRoomsStatsCommand(IInstantiator instantiator,
                                           RoomDtoStorage roomDtoStorage)
        {
            _instantiator = instantiator;
            _roomDtoStorage = roomDtoStorage;
        }

        public override void Do()
        {
            foreach (var roomDto in _roomDtoStorage.Get())
            {
                _instantiator.Instantiate<CreateRoomStatsCommand>().Execute(new CreateStatsProtocol()
                {
                    Guid = roomDto.Guid,
                    ModelID = roomDto.ModelID
                });
            }
            OnDone();
        }
    }

    public class CreateRoomStatsCommand : ICommand<CreateStatsProtocol>
    {
        private readonly RoomStatModelStorage _roomStatModelStorage;
        private readonly IInstantiator _instantiator;

        public CreateRoomStatsCommand(RoomStatModelStorage roomStatModelStorage,
                                      IInstantiator instantiator)
        {
            _roomStatModelStorage = roomStatModelStorage;
            _instantiator = instantiator;
        }

        public Task Execute(CreateStatsProtocol protocol)
        {
            var stats = new StatModel[0];
            if (_roomStatModelStorage.HasEntity(protocol.ModelID))
            {
                stats = _roomStatModelStorage.Get(protocol.ModelID).Stats;
            }
            var collectionProtocol = new CreateStatsCollectionProtocol(protocol.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<StatsCollection>>().Execute(collectionProtocol);
            return Task.CompletedTask;
        }
    }
}