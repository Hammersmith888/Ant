using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using Zenject;

namespace BugsFarm.RoomSystem
{
    public class RoomsSystem : IRoomsSystem
    {
        private class Room
        {
            public readonly string Guid;
            public readonly bool HasTasks;
            public readonly bool HasDependency;
            public readonly string ModelID;
            public readonly Func<bool> IsOpened;

            public Room(RoomSystemProtocol protocol, RoomDtoStorage dtoStorage)
            {
                Guid = protocol.Guid;
                HasTasks = protocol.HasTasks;
                HasDependency = protocol.HasDependency;
                ModelID = dtoStorage.Get(Guid).ModelID;
                IsOpened = protocol.IsOpened;
            }
        }

        private readonly IInstantiator _instantiator;
        private readonly RoomNeighboursModelStorage _neighboursStorage;

        /// <summary>
        /// arg1 = Идентификатор комнаты, arg2 = контроллер комнаты
        /// </summary>
        private readonly Dictionary<string, Room> _storage;
        
        private Room _currentRoom;
        public RoomsSystem(IInstantiator instantiator, RoomNeighboursModelStorage neighboursStorage)
        {
            _instantiator = instantiator;
            _neighboursStorage = neighboursStorage;
            _storage = new Dictionary<string, Room>();
        }

        public void Initialize()
        {
            foreach (var room in GetSortedRooms())
            {
                if (room.IsOpened())
                {
                    OpenRoom(room.Guid);
                    continue;
                }
                
                if (HasTasks(room.Guid))
                {
                    MessageBroker.Default.Publish(new OpenableRoomProtocol(room.Guid));
                    AdjacentOpenable(room.Guid);
                    return;
                }
            }
        }
        
        public void Registration(RoomSystemProtocol protocol)
        {
            if (_storage.ContainsKey(protocol.Guid))
            {
                throw new ArgumentException($"{this} : {nameof(Registration)} :: {nameof(RoomSystemProtocol)} with [Guid : {protocol.Guid}], alredy exist.");
            }

            _storage.Add(protocol.Guid, _instantiator.Instantiate<Room>(new object[]{protocol}));
        }
        
        public void UnRegistration(string guid)
        {
            if (!_storage.ContainsKey(guid))
            {
                return;
            }

            _storage.Remove(guid);
        }
        
        public void OpenRoom(string guid)
        {
            if (!HasEntity(guid))
            {
                throw new ArgumentException($"{this} : Room with guid : {guid} , does not exist.");
            }
            
            var room = _storage[guid];
            if (_currentRoom == room)
            {
                _currentRoom = null;
            }
            
            MessageBroker.Default.Publish(new OpenRoomProtocol(room.Guid));
            AdjacentOpenable(guid);
        }
        
        public bool HasEntity(string guid)
        {
            return _storage.ContainsKey(guid);
        }

        public IEnumerable<string> Opened()
        {
            return _storage.Values.Where(x => x.IsOpened()).Select(x => x.Guid);
        }

        private void AdjacentOpenable(string guid)
        {
            if (!HasEntity(guid))
            {
                throw new ArgumentException($"{this} : {nameof(AdjacentOpenable)} :: {nameof(RoomSystemProtocol)} does not exist.");
            }

            var resolverRoom = _storage[guid];
            var sortedRooms  = GetSortedRooms();

            if (_neighboursStorage.HasEntity(resolverRoom.ModelID))
            {
                var neighbours    = _neighboursStorage.Get(resolverRoom.ModelID).Neighbours;
                var adjacentRooms = sortedRooms.Where(x => neighbours.Contains(x.ModelID));
                foreach (var room in adjacentRooms)
                {
                    if (room.HasDependency && resolverRoom.IsOpened()) // зависимая открываемость
                    {
                        OpenRoom(room.Guid);
                    }
                    else if(!room.IsOpened()) // доступность для покупки комнаты
                    {
                        MessageBroker.Default.Publish(new OpenableRoomProtocol(room.Guid));
                    }
                }
            }

            if (_currentRoom != null)
            {
                return;
            }

            foreach (var room in sortedRooms)
            {
                if (HasTasks(room.Guid))
                {
                    return;   
                }
            }
        }
        
        private bool HasTasks(string guid)
        {
            if (!HasEntity(guid) || _currentRoom != null)
            {
                return false;
            }

            var room = _storage[guid];
            if (room.IsOpened() || !room.HasTasks)
            {
                return false;
            }

            _currentRoom = room;
            MessageBroker.Default.Publish(new NextRoomProtocol(room.Guid));
            return true;
        }

        private Room[] GetSortedRooms()
        {
            return _storage.Values.OrderBy(x => int.Parse(x.ModelID)).ToArray();
        }
    }
}