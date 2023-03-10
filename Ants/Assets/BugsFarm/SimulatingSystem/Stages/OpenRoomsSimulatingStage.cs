using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InventorySystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using UniRx;

namespace BugsFarm.SimulatingSystem
{
    public class OpenRoomsSimulatingStage
    {
        public int OpenedRoomsAmount => _openedRoomsAmount;
        
        private readonly SimulatingRoomGroupModelStorage _simulatingRoomGroupModelStorage;
        private readonly RoomNeighboursModelStorage _neighboursModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private SimulatingMudStockCreator _simulatingMudStockCreator;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly RoomModelStorage _roomModelStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly RoomDtoStorage _roomDtoStorage;
        private readonly PrefabLoader _prefabLoader;

        private List<ISimulatingRoom> _closedRoomsList;
        private Dictionary<string, List<SimulatingEntityDto>> _simulatingData;
        private float _minutesInCycle;
        private int _openedRoomsAmount;

        private const string _prefix = "PlaceIDGroup_";

        public OpenRoomsSimulatingStage(UnitDtoStorage unitDtoStorage,
                                        InventoryDtoStorage inventoryDtoStorage,
                                        RoomDtoStorage roomDtoStorage,
                                        RoomModelStorage roomModelStorage,
                                        SimulatingRoomGroupModelStorage simulatingRoomGroupModelStorage,
                                        PrefabLoader prefabLoader,
                                        SimulatingMudStockCreator simulatingMudStockCreator,
                                        RoomNeighboursModelStorage neighboursModelStorage,
                                        StatsCollectionStorage statsCollectionStorage)
        {
            _statsCollectionStorage = statsCollectionStorage;
            _neighboursModelStorage = neighboursModelStorage;
            _simulatingMudStockCreator = simulatingMudStockCreator;
            _prefabLoader = prefabLoader;
            _simulatingRoomGroupModelStorage = simulatingRoomGroupModelStorage;
            _roomDtoStorage = roomDtoStorage;
            _roomModelStorage = roomModelStorage;
            _inventoryDtoStorage = inventoryDtoStorage;
            _unitDtoStorage = unitDtoStorage;
            _closedRoomsList = new List<ISimulatingRoom>();
        }

        public void OpenRooms(float minutesInCycle, Dictionary<string, List<SimulatingEntityDto>> simulationData, float dayModifier)
        {
            if (!simulationData.ContainsKey(SimulatingEntityType.Unit))
                return;
            if (!simulationData.ContainsKey(SimulatingEntityType.Room))
                return;
            if (simulationData[SimulatingEntityType.Unit].All(x => _unitDtoStorage.Get(x.Guid).ModelID != "8"))
                return;

            _minutesInCycle = minutesInCycle;
            _simulatingData = simulationData;
            
            var workers = simulationData[SimulatingEntityType.Unit]
                .Where(x => _unitDtoStorage.Get(x.Guid).ModelID == "8");

            ConfigureRoomsByNeighbours();

            if (_openedRoomsAmount >= _roomDtoStorage.Count || _closedRoomsList.Count == 0)
                return;
            
            OpenClosedRooms(workers.Count(), dayModifier);
        }

        public void Dispose()
        {
            _closedRoomsList.Clear();
        }
        
        private void OpenClosedRooms(int count, float dayModifier)
        {
            for (int i = 0, j = 0; i < count; i++)
            {
                if (_closedRoomsList[j].Group == SimulatingRoomGroups.Dig)
                {
                    string guid = _closedRoomsList[j].Guid;
                    if(!_simulatingMudStockCreator.HasFreePlace() && !_simulatingMudStockCreator.IsRegistered(guid))
                        return;
                        
                    if(!_simulatingMudStockCreator.IsRegistered(guid))
                        _simulatingMudStockCreator.ReserveMudStock(guid, _closedRoomsList[j].Capacity);
                
                    _simulatingMudStockCreator.AddToStock(guid, _closedRoomsList[j].UpProgress(50.0f * dayModifier));
                }
                else
                {
                    _closedRoomsList[j].UpProgress(50.0f * dayModifier);
                }

                if (_closedRoomsList[j].IsOpened())
                {
                    NotifyRoomOpening(_closedRoomsList[j].ModelID);
                    _closedRoomsList.Remove(_closedRoomsList[j]);
                    _openedRoomsAmount++;
                    if (_closedRoomsList.Count == 0)
                        break;
                }
            }
        }

        private void NotifyRoomOpening(string modelID)
        {
            var placePrefab = _prefabLoader.Load(_prefix + modelID);
            var newMudPlaces = placePrefab.GetComponentsInChildren<PlaceID>(true);

            for (int i = 0; i < newMudPlaces.Length; i++)
            {
                var newMudPlaceIds = newMudPlaces[i].GetComponentsInChildren<APlace>(true).Select(x => x.ModelID);
                MessageBroker.Default.Publish(new OpenedPlaceIDProtocol()
                {
                    PlaceNum = newMudPlaces[i].PlaceNumber,
                    APlaceModels = newMudPlaceIds
                });
            }
        }

        private void ConfigureRoomsByNeighbours()
        {
            if (_closedRoomsList.Count > 0)
                return;

            _openedRoomsAmount = 0;
            
            List<SimulatingEntityDto> rooms = _simulatingData[SimulatingEntityType.Room];
            ISimulatingRoom[] simulatingRooms = new ISimulatingRoom[rooms.Count]; 
            for (int i = 0; i < rooms.Count; i++)
            {
                var modelID = _roomDtoStorage.Get(rooms[i].Guid).ModelID;
                var typeName = _roomModelStorage.Get(modelID).TypeName;
                var group = _simulatingRoomGroupModelStorage.Get(typeName).Group;

                var room = GetRoom(group, rooms[i].Guid, modelID);
                
                if (room.IsOpened())
                {
                    _openedRoomsAmount++;
                }
                simulatingRooms[i] = room;
            }

            simulatingRooms = simulatingRooms.OrderBy(x => int.Parse(x.ModelID)).ToArray();
            
            for (int i = 0; i < simulatingRooms.Length; i++)
            {
                if(!_neighboursModelStorage.HasEntity(i.ToString()))
                    continue;
                var neighbours = _neighboursModelStorage.Get(simulatingRooms[i].ModelID).Neighbours;
                for (int j = 0; j < neighbours.Length; j++)
                {
                    var neighbour = simulatingRooms[int.Parse(neighbours[j])];
                    if(!_closedRoomsList.Contains(neighbour) && !neighbour.IsOpened())
                        _closedRoomsList.Add(neighbour);
                }
            }
            _closedRoomsList = _closedRoomsList.OrderBy(x => int.Parse(x.ModelID)).ToList();
        }
        
        private ISimulatingRoom GetRoom(string group, string guid, string modelID)
        {
            switch(group)
            {
                case SimulatingRoomGroups.Dig:
                    return new DigSimulatingRoom(guid, modelID, _inventoryDtoStorage.Get(guid));
                case SimulatingRoomGroups.Vine:
                    return new VineSimulatingRoom(guid, modelID, _inventoryDtoStorage.Get(guid));
                case SimulatingRoomGroups.Openable:
                    return new OpenableSimulatingRoom(guid, modelID, _statsCollectionStorage.Get(guid));
                default: 
                    throw new ArgumentOutOfRangeException(nameof(group), group, null);
            };
        }
    }

    [Serializable]
    public struct OpenedPlaceIDProtocol
    {
        public string PlaceNum;
        public IEnumerable<string> APlaceModels;
    }
}