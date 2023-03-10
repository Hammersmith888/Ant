using System.Collections.Generic;
using System.Linq;
using BugsFarm.BuildingSystem;
using BugsFarm.UnitSystem;

namespace BugsFarm.SimulatingSystem
{
    public class UnitsRepositionSimulatingStage
    {
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly PathHelper _pathHelper;

        public UnitsRepositionSimulatingStage(BuildingSceneObjectStorage buildingSceneObjectStorage,
                                              UnitMoverStorage unitMoverStorage,
                                              PathHelper pathHelper,
                                              UnitDtoStorage unitDtoStorage,
                                              UnitTaskProcessorStorage unitTaskProcessorStorage)
        {
            _pathHelper = pathHelper;
            _unitMoverStorage = unitMoverStorage;
            _unitDtoStorage = unitDtoStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
        }

        public void RelocateUnits(Dictionary<string, List<SimulatingEntityDto>> simulatingData)
        {
            if (simulatingData == null || !simulatingData.ContainsKey(SimulatingEntityType.Unit))
                return;
            if (_buildingSceneObjectStorage.Count == 0)
                return;

            
            foreach (var unit in simulatingData[SimulatingEntityType.Unit])
            {
                var modelID = _unitDtoStorage.Get(unit.Guid).ModelID;

                var mover = _unitMoverStorage.Get(unit.Guid);
                var pathHelperQuery = PathHelperQuery.Empty()
                                                     .UseGraphMask("RestTask")
                                                     .UseLimitationsCheck(modelID)
                                                     .UseTraversableCheck(mover.TraversableTags);
                var positions = _pathHelper.GetRandomNodes(pathHelperQuery);
                if(positions == null || !positions.Any())
                    continue;
                
                var position =  positions.First();
                mover.SetPosition(position);
                var processor = _unitTaskProcessorStorage.Get(unit.Guid);
                processor.Interrupt();
            }
        }

    }
}