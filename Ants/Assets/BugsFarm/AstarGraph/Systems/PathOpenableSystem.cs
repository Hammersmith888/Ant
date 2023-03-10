using System;
using BugsFarm.RoomSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.AstarGraph
{
    public class PathOpenableSystem : IDisposable, IInitializable
    {
        private readonly IRoomsSystem _roomsSystem;
        private readonly IPointGraph _pointGraph;
        private readonly RoomDtoStorage _roomDtoStorage;
        private IDisposable _openRoomEvent;

        public PathOpenableSystem(IRoomsSystem roomsSystem, 
                                  IPointGraph pointGraph,
                                  RoomDtoStorage roomDtoStorage)
        {
            _roomsSystem = roomsSystem;
            _pointGraph = pointGraph;
            _roomDtoStorage = roomDtoStorage;
        }

        public void Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
        }
        
        public void Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
        }
        
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (!_roomDtoStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{nameof(RoomDto)} with [Guid : {protocol.Guid}], does not exist.");
            }

            var dto = _roomDtoStorage.Get(protocol.Guid);
            _pointGraph.OpenGroupe(dto.ModelID);
        }
    }
}