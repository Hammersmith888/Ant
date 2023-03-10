using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StatsService;
using BugsFarm.UnitSystem;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class GrowLarvaSimulatingStage
    {
        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly SimulatingAntLarvaCreator _antLarvaCreator;
        private readonly UnitStatModelStorage _unitStatModelStorage;
        private readonly UnitMoverDtoStorage _unitMoverDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly IInstantiator _instantiator;

        public GrowLarvaSimulatingStage(ISimulatingEntityStorage simulatingEntityStorage,
                                        SimulatingAntLarvaCreator antLarvaCreator,
                                        UnitModelStorage unitModelStorage,
                                        UnitDtoStorage unitDtoStorage,
                                        UnitMoverDtoStorage unitMoverDtoStorage,
                                        UnitStatModelStorage unitStatModelStorage,
                                        IInstantiator instantiator)
        {
            _instantiator = instantiator;
            _unitStatModelStorage = unitStatModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _unitMoverDtoStorage = unitMoverDtoStorage;
            _unitModelStorage = unitModelStorage;
            _antLarvaCreator = antLarvaCreator;
            _simulatingEntityStorage = simulatingEntityStorage;
        }

        public void GrowLarvas(float minutesInCycle, List<string> larvasToDelete,
            List<SimulatingSpawnUnitDto> spawnUnitDtos, Dictionary<string, List<SimulatingEntityDto>> simulationData)
        {
            foreach (var growingLarva in _antLarvaCreator.ReservedPlaces.Values.ToArray())
            {
                growingLarva.GrowTimeLeft -= minutesInCycle;
                if(growingLarva.GrowTimeLeft > 0)
                    continue;
                DeleteLarva(growingLarva, larvasToDelete);
                CreateUnit(growingLarva, spawnUnitDtos, simulationData);
            }
        }

        private void CreateUnit(SimulatingLarvaData growingLarva, List<SimulatingSpawnUnitDto> simulatingSpawnUnitDtos,
            Dictionary<string, List<SimulatingEntityDto>> simulationData)
        {
            var protocol = new CreateUnitProtocol(growingLarva.ToBornModelID, true);
            var model = _unitModelStorage.Get(protocol.ModelID);
            var nameID = UnitsUtils.GenerateNameKey(model.IsFemale);
            UnitDto unitDto = new UnitDto(protocol.Guid, protocol.ModelID, nameID);
            _unitDtoStorage.Add(unitDto);
            var stats = new StatModel[0];
            stats = _unitStatModelStorage.Get(unitDto.ModelID).Stats;
            var collectionProtocol = new CreateStatsCollectionProtocol(unitDto.Guid, stats);
            _instantiator.Instantiate<CreateStatsCollectionCommand<UnitStatsCollection>>().Execute(collectionProtocol);

            _unitMoverDtoStorage.Add(new UnitMoverDto{Guid = protocol.Guid, ModelID = unitDto.ModelID});
            
            var entityDto = new SimulatingEntityDto() {Guid = protocol.Guid, EntityType = SimulatingEntityType.Unit};
            
            _simulatingEntityStorage.Add(entityDto);
            if(!simulationData.ContainsKey(SimulatingEntityType.Unit))
                simulationData.Add(SimulatingEntityType.Unit, new List<SimulatingEntityDto>());
            simulationData[SimulatingEntityType.Unit].Add(entityDto);
            
            simulatingSpawnUnitDtos.Add(new SimulatingSpawnUnitDto()
            {
                Guid = protocol.Guid,
                ModelID = protocol.ModelID
            });
        }

        private void DeleteLarva(SimulatingLarvaData growingLarva, List<string> larvasToDelete)
        {
            _antLarvaCreator.ReservedPlaces.Remove(growingLarva.Guid);
            _simulatingEntityStorage.Remove(growingLarva.Guid);
            if (growingLarva.IsVirtual)
                return;
            
            larvasToDelete.Add(growingLarva.Guid);
        }
    }
}