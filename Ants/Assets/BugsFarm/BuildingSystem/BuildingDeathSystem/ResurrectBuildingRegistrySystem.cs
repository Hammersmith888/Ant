using System;
using System.Linq;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.BuildingSystem.DeathSystem
{
    public class ResurrectBuildingRegistrySystem
    {
        private readonly ResurrectBuildingDataStorage _resurrectRegistryStorage;
        private readonly StatsCollectionDtoStorage _statsCollectionDtoStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly IInstantiator _instantiator;

        public ResurrectBuildingRegistrySystem(ResurrectBuildingDataStorage resurrectRegistryStorage)
        {
            _resurrectRegistryStorage = resurrectRegistryStorage;
        }


        public void Register(ResurrectBuildingData resurrectBuildingData)
        {
            if (IsRegistered(resurrectBuildingData.Guid))
                return;
            _resurrectRegistryStorage.Add(resurrectBuildingData);
        }

        public void Unregister(string guid)
        {
            if (!IsRegistered(guid))
                return;
            _resurrectRegistryStorage.Remove(guid);
        }
        public void ResurrectAllBuildings()
        {
            foreach (var registryProtocol in _resurrectRegistryStorage.Get().ToArray())
            {
                ResurrectBuilding(registryProtocol);
            }
        }
        
        public void ResurrectBuilding(ResurrectBuildingData resurrectBuildingData)
        {
            if (!IsRegistered(resurrectBuildingData.Guid))
                return;
            _resurrectRegistryStorage.Remove(resurrectBuildingData.Guid);
            MessageBroker.Default.Publish(resurrectBuildingData);
        }

        private bool IsRegistered(string guid)
        {
            return _resurrectRegistryStorage.HasEntity(guid);
        }
        
    }
}