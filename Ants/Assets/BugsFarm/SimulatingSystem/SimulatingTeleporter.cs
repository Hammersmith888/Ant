using System;
using System.Linq;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;
using BugsFarm.RoomSystem;
using BugsFarm.UnitSystem;
using UnityEngine;

namespace BugsFarm.SimulatingSystem.AssignableTasks
{
    public class SimulatingTeleporter
    {
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitMoverStorage _unitMoverStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly UnitTaskProcessorStorage _unitTaskProcessorStorage;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly RoomSceneObjectStorage _roomSceneObjectStorage;
        private readonly PathHelper _pathHelper;

        public SimulatingTeleporter(UnitDtoStorage unitDtoStorage,
                                    UnitTaskProcessorStorage unitTaskProcessorStorage, 
                                    UnitMoverStorage unitMoverStorage,
                                    PathHelper pathHelper,
                                    BuildingSceneObjectStorage buildingSceneObjectStorage,
                                    RoomSceneObjectStorage roomSceneObjectStorage,
                                    BuildingDtoStorage buildingDtoStorage)
        {
            _roomSceneObjectStorage = roomSceneObjectStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _unitTaskProcessorStorage = unitTaskProcessorStorage;
            _unitMoverStorage = unitMoverStorage;
            _pathHelper = pathHelper;
            _buildingDtoStorage = buildingDtoStorage;
            _unitDtoStorage = unitDtoStorage;
        }

        public void TeleportTo(string unitGuid, Vector2 position)
        {
            var processor = _unitTaskProcessorStorage.Get(unitGuid);
                // processor.Interrupt();
            var mover = _unitMoverStorage.Get(unitGuid);
            mover.Stay();
            mover.SetPosition(position);
            processor.Interrupt();
        }

        public INode TeleportToRandom(string guid)
        {
            var modelID = _unitDtoStorage.Get(guid).ModelID;

            var mover = _unitMoverStorage.Get(guid);
            var pathHelperQuery = PathHelperQuery.Empty()
                .UseGraphMask("RestTask")
                .UseLimitationsCheck(modelID)
                .UseTraversableCheck(mover.TraversableTags);
            var positions = _pathHelper.GetRandomNodes(pathHelperQuery);
            if(positions == null || !positions.Any())
                return null;
                
            var position =  positions.First();
            mover.SetPosition(position);
            var processor = _unitTaskProcessorStorage.Get(guid);
            processor.Interrupt();
            return position;
        }
        
        public void TeleportToAny(string unitGuid, string buildingModelId)
        {
            var buildingDtos = _buildingDtoStorage.Get().Where(x => x.ModelID == buildingModelId);
            var buildingDto = buildingDtos.ElementAt(UnityEngine.Random.Range(0, buildingDtos.Count()));
            var buildingSceneObject = _buildingSceneObjectStorage.Get(buildingDto.Guid);
            if (!buildingSceneObject.TryGetComponent(out TasksPoints points))
                return;
            var positionSide = points.Points.First();
            TeleportTo(unitGuid, positionSide.Position);
        }

        public void TeleportToConcrete(string unitGuid, string buildingGuid)
        {
            var dto = _buildingDtoStorage.Get(buildingGuid);
            var buildingSceneObject = _buildingSceneObjectStorage.Get(dto.Guid);
            if (!buildingSceneObject.TryGetComponent(out TasksPoints points))
                return;
            var positionSide = points.Points.First();
            TeleportTo(unitGuid, positionSide.Position);
        }

        public void TeleportToConcreteRoom(string unitGuid, string roomGuid)
        {
            var sceneObject = _roomSceneObjectStorage.Get(roomGuid);
            if (!sceneObject.TryGetComponent(out TasksPoints points))
                return;
            var positionSide = points.Points.First();
            TeleportTo(unitGuid, positionSide.Position);
        }
    }
}