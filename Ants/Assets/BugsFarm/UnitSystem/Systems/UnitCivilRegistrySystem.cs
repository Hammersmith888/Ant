using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Graphic;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using UniRx;
using Zenject;

namespace BugsFarm.UnitSystem
{ 
    public class UnitCivilRegistrySystem
    {
        private readonly UnitCivilRegistryDtoStorage _storage;
        private readonly IMonoPool _monoPool;
        private readonly Dictionary<string, UnitRipSceneObject> _rips;
        private readonly StatsCollectionDtoStorage _statsCollectionDtoStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly IInstantiator _instantiator;
        private readonly UnitDtoStorage _unitDtoStorage;
        private readonly UnitModelStorage _unitModelStorage;
        private readonly UnitMoverDtoStorage _unitMoverDtoStorage;

        public UnitCivilRegistrySystem(UnitCivilRegistryDtoStorage storage, 
                                       IMonoPool monoPool,
                                       ISimulationSystem simulationSystem,
                                       IInstantiator instantiator,
                                       StatsCollectionDtoStorage statsCollectionDtoStorage,
                                       UnitDtoStorage unitDtoStorage,
                                       UnitModelStorage unitModelStorage,
                                       UnitMoverDtoStorage unitMoverDtoStorage)
        {
            _unitMoverDtoStorage = unitMoverDtoStorage;
            _unitModelStorage = unitModelStorage;
            _unitDtoStorage = unitDtoStorage;
            _statsCollectionDtoStorage = statsCollectionDtoStorage;
            _instantiator = instantiator;
            _simulationSystem = simulationSystem;
            _storage = storage;
            _monoPool = monoPool;
            _rips = new Dictionary<string, UnitRipSceneObject>();
        }
        public void Registration(UnitCivilRegistryDto registry)
        {
            if (HasUnit(registry.Id))
            {
                throw new InvalidOperationException($"{this} : Registration :: Unit with [Guid : {registry.Id}], alredy exist.");
            }
            _storage.Add(registry);
            SpawnRip(registry, registry.PostLoad);
        }
        public void UnRegistration(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            if (_rips.ContainsKey(guid))
            {
                var rip = _rips[guid];
                _rips.Remove(guid);
                _monoPool.Despawn(rip);
            }
            _storage.Remove(guid);
        }
        public void ResurrectAllUnits()
        {
            foreach (var deadUnits in _storage.Get().ToArray())
            {
                ResurrectUnit(deadUnits.Id);
            }
        }
        public void ResurrectUnit(string guid)
        {
            if (!HasUnit(guid))
            {
                return;
            }

            RespawnUnit(guid);
        }

        private void RespawnUnit(string guid)
        {
            var civilData = _storage.Get(guid);

            _statsCollectionDtoStorage.Add(civilData.UnitStatsDto);
            _unitMoverDtoStorage.Add(civilData.MoverDto);
            _unitDtoStorage.Add(civilData.UnitDto);

            var creatingProtocol = new CreateUnitProtocol(guid, false);
            _instantiator.Instantiate<CreateUnitCommand>().Execute(creatingProtocol);
            var spawnProtocol = new UnitSpawnProtocol(civilData.Id, civilData.MoverDto.Position);
            _instantiator.Instantiate<UnitSpawnCommand<SpawnFromAlphaTask>>().Execute(spawnProtocol);
            
            UnRegistration(guid);
        }

        public bool HasUnit(string guid)
        {
            return _storage.HasEntity(guid);
        }

        public void SpawnRip(UnitCivilRegistryDto registry, bool postLoad = false)
        {
            if(_rips.ContainsKey(registry.Id)) return;
            
            var unitRipSceneObject = _monoPool.Spawn<UnitRipSceneObject>();
            bool isAppearingInstanly = postLoad || _simulationSystem.Simulation;
            unitRipSceneObject.SetGuid(registry.Id);
            unitRipSceneObject.SetActive(true);
            unitRipSceneObject.SetAlpha( isAppearingInstanly ? 1 : 0);
            unitRipSceneObject.SetAngle(registry.MoverDto.Normal);
            unitRipSceneObject.SetPosition(registry.MoverDto.Position);
            unitRipSceneObject.SetLayer(registry.MoverDto.Layer);
            unitRipSceneObject.SetInteraction(postLoad);
            if(!isAppearingInstanly)
                unitRipSceneObject.FadeOut();
            _rips.Add(registry.Id, unitRipSceneObject);
        }


    }
}