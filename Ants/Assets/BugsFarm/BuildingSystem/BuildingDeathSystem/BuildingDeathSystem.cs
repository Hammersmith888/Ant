using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    public class BuildingDeathSystem : BaseDeathSystem<DeathBuildingProtocol>
    {
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly ResurrectBuildingRegistrySystem _resurrectBuildingRegistrySystem;

        public BuildingDeathSystem(IInstantiator instantiator,
                                   BuildingDtoStorage buildingDtoStorage,
                                   ResurrectBuildingRegistrySystem resurrectBuildingRegistrySystem) : base(instantiator)
        {
            _resurrectBuildingRegistrySystem = resurrectBuildingRegistrySystem;
            _buildingDtoStorage = buildingDtoStorage;
        }
        
        public override void Unregister(string guid)
        {
            DisposeDeathController(guid);

            _storage.Remove(guid);
            _deadObjects.Remove(guid);
            _dyingObjects.Remove(guid);
        }

        private void StartDyingProcess(string guid)
        {
            if (IsRunned(guid)) return;

            DisposeDeathController(guid);
            
            BuildingDto buildingDto = _buildingDtoStorage.Get(guid);

            _resurrectBuildingRegistrySystem.Register(new ResurrectBuildingData() {Guid = guid, ModelID = buildingDto.ModelID});
            
            _deadObjects.Remove(buildingDto.Guid);
            _dyingObjects.Remove(buildingDto.Guid);
            MessageBroker.Default.Publish(new PostDeathBuildingProtocol() {ModelID = buildingDto.ModelID, Guid = buildingDto.Guid});
        }
        
        protected override void OnDeadly(DeathBuildingProtocol deathBuildingProtocol)
        {
            if (!IsRegistered(deathBuildingProtocol.Guid)) return;
            if (IsDead(deathBuildingProtocol.Guid)) return;
            
            _deadObjects.Add(deathBuildingProtocol.Guid, deathBuildingProtocol.DeathReason);
            StartDyingProcess(deathBuildingProtocol.Guid);
        }
        
        private void DisposeDeathController(string guid)
        {
            if (!IsRegistered(guid))
                return;
            
            _storage[guid].Dispose();
        }
    }
}
