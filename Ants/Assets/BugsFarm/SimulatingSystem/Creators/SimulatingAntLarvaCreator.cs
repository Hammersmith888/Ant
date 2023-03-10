using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.SimulationSystem;
using BugsFarm.UnitSystem;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class  SimulatingAntLarvaCreator : ISavable
    {
        public Dictionary<string, SimulatingLarvaData> ReservedPlaces => _reservedPlaces;

        private readonly IInstantiator _instantiator;
        private readonly BuildingSceneObjectStorage _buildingSceneObjectStorage;
        private readonly ISimulatingEntityStorage _simulatingEntityStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly SceneEntityStorage _sceneEntityStorage;
        private readonly UnitStatModelStorage _statModelStorage;
        private readonly BuildingDtoStorage _buildingDtoStorage;
        private readonly ISimulationSystem _simulationSystem;
        private readonly ISavableStorage _savableStorage;
        private readonly UnitDtoStorage _unitDtoStorage;

        private Dictionary<string, SimulatingLarvaData> _reservedPlaces;
        private List<Vector2> _freePositions;
        private readonly IDisposable _onGameReloadsEvent;
        private LarvaCreatorSaveData _larvaSaveData;

        private const string _antLarvaModelID = "9";
        private const string _queenModelID = "54";
        private const string _bornTimeStatKey = "stat_bornTime";
        private const string _birthModelIDStatKey = "stat_birthModelID";
        private const string _growthTimeStatKey = "stat_growthTime";
        private const string _larvaStageStatKey = "stat_stage";
        
        public SimulatingAntLarvaCreator(IInstantiator instantiator, 
                                        StatsCollectionStorage statsCollectionStorage,
                                        ISavableStorage savableStorage,
                                        ISimulatingEntityStorage simulatingEntityStorage,
                                        UnitDtoStorage unitDtoStorage,
                                        BuildingSceneObjectStorage buildingSceneObjectStorage,
                                        BuildingDtoStorage buildingDtoStorage,
                                        ISimulationSystem simulationSystem,
                                        UnitStatModelStorage statModelStorage)
        {
            _statModelStorage = statModelStorage;
            _simulationSystem = simulationSystem;
            _buildingDtoStorage = buildingDtoStorage;
            _buildingSceneObjectStorage = buildingSceneObjectStorage;
            _unitDtoStorage = unitDtoStorage;
            _instantiator = instantiator;
            _savableStorage = savableStorage;
            _simulatingEntityStorage = simulatingEntityStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _reservedPlaces = new Dictionary<string, SimulatingLarvaData>();
            _onGameReloadsEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);
            _savableStorage.Register(this);
        }

        private void OnGameReloading(GameReloadingReport report)
        {
            _onGameReloadsEvent.Dispose();
            _savableStorage.Unregister(this);
        }
        public bool HasFreePlace()
        {
            return _freePositions.Count > 0;
        }

        public bool IsRegistered(string guid)
        {
            return _reservedPlaces.ContainsKey(guid);
        }
        
        public void RegisterNewLarva(string guid, string modelID, string toBornModelID, float growTimeLeft, bool isVirtual, float minutesInCycle)
        {
            if (!HasFreePlace())
                return;
  
            var position = _freePositions[0];
            _freePositions.RemoveAt(0);

            double bornTime = 0.0;

            if (isVirtual)
            {
                _simulatingEntityStorage.Add(new SimulatingEntityDto(){Guid = guid, EntityType = SimulatingEntityType.Larva});
                _unitDtoStorage.Add(new UnitDto(guid, modelID, 1));
                bornTime = _simulationSystem.GameAge + minutesInCycle * 60;
            }
            else
            {
                var statCollection = _statsCollectionStorage.Get(guid);
                var statBorn = statCollection.Get<StatModifiable>(_bornTimeStatKey);
                bornTime = statBorn.Value;
                var statGrowth = statCollection.Get<StatVital>(_growthTimeStatKey);
                var statStage = statCollection.Get<StatVital>(_larvaStageStatKey);
                statGrowth.CurrentValue = Mathf.Max(0,statGrowth.Value * 60.0f * (statStage.Value - 1) - (float)(_simulationSystem.GameAge - bornTime)) / 60.0f - statGrowth.Value * (statStage.Value - statStage.CurrentValue + 1);
            }
            
            _reservedPlaces.Add(guid, new SimulatingLarvaData()
            {
                Guid = guid,
                ModelID = modelID,
                Position = position,
                ToBornModelID = toBornModelID,
                GrowTimeLeft = growTimeLeft,
                BornTime = bornTime,
                IsVirtual = isVirtual
            });
        }
        
        public void CreateLarvasSceneObjects()
        {
            foreach (var place in _reservedPlaces.Values)
            {
                if(!place.IsVirtual)
                    continue;
                
                _unitDtoStorage.Remove(place.Guid);
                
                var unitBuildingProtocol = new CreateUnitProtocol(place.ModelID, true);
                
                var stats = new StatModel[0];
                stats = _statModelStorage.Get(place.ModelID).Stats;
                var collectionProtocol = new CreateStatsCollectionProtocol(place.Guid, stats);
                _instantiator.Instantiate<CreateStatsCollectionCommand<UnitStatsCollection>>().Execute(collectionProtocol);
                
                var statCollection = _statsCollectionStorage.Get(place.Guid);
                var statGrowth = statCollection.Get<StatVital>(_growthTimeStatKey);
                var statStage = statCollection.Get<StatVital>(_larvaStageStatKey);
                statGrowth.CurrentValue = Mathf.Max(0,statGrowth.Value * 60.0f * (statStage.Value - 1) - (float)(_simulationSystem.GameAge - place.BornTime)) / 60.0f - statGrowth.Value * (statStage.Value - statStage.CurrentValue + 1);

                
                _instantiator.Instantiate<CreateUnitCommand>().Execute(unitBuildingProtocol);
                place.Guid = unitBuildingProtocol.Guid;
                statCollection.AddModifier(_birthModelIDStatKey, new StatModBaseAdd(int.Parse(place.ToBornModelID)));
                statCollection.RemoveAllModifiers(_bornTimeStatKey);
                statCollection.AddModifier(_bornTimeStatKey, new StatModBaseAdd((float)place.BornTime));
                var spawnProtocol = new UnitSpawnProtocol(unitBuildingProtocol.Guid, place.Position);
                _instantiator.Instantiate<UnitSpawnCommand<SpawnFromAlphaTask>>().Execute(spawnProtocol);
            }
        }
        
        public string GetTypeKey()
        {
            return GetType().ToString();
        }
        public string Save()
        {

            if (_larvaSaveData == null)
            {
                var queenDto = _buildingDtoStorage.Get().FirstOrDefault(x => x.ModelID == _queenModelID);
                if (queenDto == null)
                    return null;

                var sceneObject = _buildingSceneObjectStorage.Get(queenDto.Guid);
                var points = sceneObject.GetComponent<LarvaPoints>();
                
                LarvaSaveData[] saveDatas = new LarvaSaveData[points.Points.Count()];
                var larvaCount = _unitDtoStorage.Get().Count(x => x.ModelID == _antLarvaModelID);
                var pointsArray = points.Points.ToArray();
                for (int i = 0; i < pointsArray.Length; i++)
                {
                    var position = pointsArray[i].Position;
                    saveDatas[i] = new LarvaSaveData()
                    {
                        PosX = position.x,
                        PosY = position.y
                    };
                }
            
                return JsonHelper.ToJson(new LarvaCreatorSaveData()
                {
                    LarvaPositions = saveDatas,
                    LarvasCount = larvaCount,
                });
            }
            
            _larvaSaveData.LarvasCount = _unitDtoStorage.Get().Count(x => x.ModelID == _antLarvaModelID);
            return JsonHelper.ToJson(_larvaSaveData);
   
        }
        
        public void Load(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData)) return;

            _larvaSaveData = JsonHelper.FromJson<LarvaCreatorSaveData>(jsonData)[0];
            var freePositionsArray = _larvaSaveData.LarvaPositions.Skip(_larvaSaveData.LarvasCount).Select(x => new Vector2(x.PosX, x.PosY));
             _freePositions = freePositionsArray.ToList();
        }
    }

    [Serializable]
    public class LarvaCreatorSaveData
    {
        public LarvaSaveData[] LarvaPositions;
        public int LarvasCount;
    }
    
    [Serializable]
    public struct LarvaSaveData
    {
        public float PosX;
        public float PosY;
    }
    public class SimulatingLarvaData
    {
        public string Guid;
        public string ModelID;
        public string ToBornModelID;
        public Vector2 Position;
        public float GrowTimeLeft;
        public double BornTime;
        public bool IsVirtual;
    }
}