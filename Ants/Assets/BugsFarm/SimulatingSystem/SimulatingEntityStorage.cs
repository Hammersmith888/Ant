using System.Collections.Generic;
using BugsFarm.BuildingSystem;
using BugsFarm.RoomSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingEntityStorage : ISimulatingEntityStorage
    {
        private Dictionary<string, SimulatingEntityDto> _registeredEntities;
        private readonly UnitsSimulationGroupModelStorage _unitsSimulationGroupModelStorage;
        private readonly BuildingSimulationGroupStorage _buildingSimulationGroupStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly RoomDtoStorage _roomDtoStorage;

        public SimulatingEntityStorage(BuildingSimulationGroupStorage buildingSimulationGroupStorage,
                                       BuildingDtoStorage buildingDtoStorage,
                                       UnitDtoStorage unitDtoStorage,
                                       UnitsSimulationGroupModelStorage unitsSimulationGroupModelStorage,
                                       RoomDtoStorage roomDtoStorage)
        {
            _roomDtoStorage = roomDtoStorage;
            _unitsSimulationGroupModelStorage = unitsSimulationGroupModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingSimulationGroupStorage = buildingSimulationGroupStorage;
            _registeredEntities = new Dictionary<string, SimulatingEntityDto>();
        }

        public void ConstructData()
        {
            ProcessBuildings();
            ProcessUnits();
            ProcessRooms();
        }

        private void ProcessRooms()
        {
            foreach (var roomDto in _roomDtoStorage.Get())    
            {
                _registeredEntities.Add(roomDto.Guid, new SimulatingEntityDto()
                {
                    Guid = roomDto.Guid,
                    EntityType = SimulatingEntityType.Room
                });
            }
        }

        public void Remove(string guid)
        {
            _registeredEntities.Remove(guid);
        }

        public void Add(SimulatingEntityDto entityDto)
        {
            _registeredEntities.Add(entityDto.Guid, entityDto);
        }
        private void ProcessUnits()
        {
            foreach (var unitDto in _unitDtoStorage.Get())
            {
                if(!_unitsSimulationGroupModelStorage.HasEntity(unitDto.ModelID))
                    continue;

                var entityGroup = _unitsSimulationGroupModelStorage.Get(unitDto.ModelID).SimulationGroup;

                _registeredEntities.Add(unitDto.Guid, new SimulatingEntityDto()
                {
                    Guid = unitDto.Guid,
                    EntityType = entityGroup
                });
            }
        }

        private void ProcessBuildings()
        {
            foreach (var buildingDto in _buildingDtoStorage.Get())
            {
                if(!_buildingSimulationGroupStorage.HasEntity(buildingDto.ModelID))
                    continue;
                
                var entityGroup = _buildingSimulationGroupStorage.Get(buildingDto.ModelID).SimulationGroup;
                
                _registeredEntities.Add(buildingDto.Guid, new SimulatingEntityDto()
                {
                    Guid = buildingDto.Guid,
                    EntityType = entityGroup
                });
            }
        }

        public Dictionary<string, List<SimulatingEntityDto>> CreateTemporaryDatabase()
        {
            Dictionary<string, List<SimulatingEntityDto>> temporalCollection =
                new Dictionary<string, List<SimulatingEntityDto>>();

            foreach (var entityDto in _registeredEntities)
            {
                var simulatingType = entityDto.Value.EntityType;
                if(!temporalCollection.ContainsKey(simulatingType))
                    temporalCollection.Add(simulatingType, new List<SimulatingEntityDto>());
                temporalCollection[simulatingType].Add(entityDto.Value);
            }

            return temporalCollection;
        }


    }
}