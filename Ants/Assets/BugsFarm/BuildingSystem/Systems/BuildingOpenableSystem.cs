using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.RoomSystem;
using BugsFarm.Services.SaveManagerService;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    [Serializable]
    public class BuildingOpenableSystem : IDisposable, IInitializable, ISavable
    {
        [NonSerialized] private readonly IInstantiator _instantiator;
        [NonSerialized] private readonly BuildingOpenableModelStorage _buildingOpenableModelStorage;
        [NonSerialized] private readonly RoomDtoStorage _roomDtoStorage;
        [SerializeField] private List<string> _spawned;
        private IDisposable _openRoomEvent;
        public BuildingOpenableSystem(IInstantiator instantiator,
                                      ISavableStorage savableStorage,
                                      BuildingOpenableModelStorage buildingOpenableModelStorage,
                                      RoomDtoStorage roomDtoStorage)
        {
            _instantiator = instantiator;
            _buildingOpenableModelStorage = buildingOpenableModelStorage;
            _roomDtoStorage = roomDtoStorage;
            savableStorage.Register(this);
            _spawned = new List<string>();
        }

        public void Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
        }

        public bool HasOpenedRooms()
        {
            return _spawned.Count != _roomDtoStorage.Get().Count();
        }
        
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (!_roomDtoStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{nameof(RoomDto)} with [Guid : {protocol.Guid}], does not exist.");
            }

            var roomDto = _roomDtoStorage.Get(protocol.Guid);
            if (!_buildingOpenableModelStorage.HasEntity(roomDto.ModelID) || _spawned.Contains(roomDto.ModelID))
            {
                return;
            }
            var model = _buildingOpenableModelStorage.Get(roomDto.ModelID);
            var createCommand = _instantiator.Instantiate<CreateBuildingCommand>();
            foreach (var item in model.Items)
            {
                createCommand.Execute(new CreateBuildingProtocol(item.ModelID, item.PlaceNum,true,true));
            }
            _spawned.Add(model.RoomID);
        }

        public void Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
        }

        public string GetTypeKey()
        {
            return GetType().Name;
        }

        public string Save()
        {
            return JsonHelper.ToJson(this);
        }

        public void Load(string jsonData)
        {
            var data = JsonHelper.FromJson<BuildingOpenableSystem>(jsonData);
            if(data == null || data.Length == 0) return;
            _spawned = data[0]._spawned;
        }
    }
}