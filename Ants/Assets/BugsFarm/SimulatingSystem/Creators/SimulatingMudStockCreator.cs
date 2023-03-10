using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InventorySystem;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.SaveManagerService;
using BugsFarm.Services.StatsService;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.SimulatingSystem
{
    public class SimulatingMudStockCreator : ISavable
    {
        private readonly IInstantiator _instantiator;
        private readonly BuildingStatModelStorage _buildingStatModelStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IReservedPlaceSystem _reservedPlaceSystem;
        private readonly InventoryDtoStorage _inventoryDtoStorage;
        private readonly ISavableStorage _savableStorage;
        private readonly IDisposable _onGameReloadsEvent;
        private readonly IDisposable _onRoomOpenedEvent;
        private readonly PlaceIdStorage _placeIdStorage;
        private readonly ISaveManager _saveManager;

        private Dictionary<string, SimulatingReservedMudStock> _reservedMudStocks;
        private List<string> _freePlaceIds;

        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _resourceMudStatKey = "stat_maxResource";
        private const string _preProductionMudStatKey = "stat_preProduction";
        private const string _prefix = "PlaceIDGroup_";
        private const string _mudStockModelID = "49";
        
        public SimulatingMudStockCreator(IInstantiator instantiator, 
                                         StatsCollectionStorage statsCollectionStorage,
                                         PlaceIdStorage placeIdStorage,
                                         ISavableStorage savableStorage,
                                         IReservedPlaceSystem reservedPlaceSystem,
                                         InventoryDtoStorage inventoryDtoStorage,
                                         BuildingStatModelStorage buildingStatModelStorage,
                                         ISaveManager saveManager)
        {
            _saveManager = saveManager;
            _buildingStatModelStorage = buildingStatModelStorage;
            _inventoryDtoStorage = inventoryDtoStorage;
            _reservedPlaceSystem = reservedPlaceSystem;
            _savableStorage = savableStorage;
            _placeIdStorage = placeIdStorage;
            _instantiator = instantiator;
            _statsCollectionStorage = statsCollectionStorage;
            _reservedMudStocks = new Dictionary<string, SimulatingReservedMudStock>();
            _onRoomOpenedEvent = MessageBroker.Default.Receive<OpenedPlaceIDProtocol>().Subscribe(OnRoomOpened);
            _onGameReloadsEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);
            _savableStorage.Register(this);
        }

        private void OnGameReloading(GameReloadingReport report)
        {
            _onGameReloadsEvent.Dispose();
            _onRoomOpenedEvent.Dispose();
            _savableStorage.Unregister(this);
        }
        public bool HasFreePlace()
        {
            return _freePlaceIds.Count > 0;
        }

        public bool IsRegistered(string guid)
        {
            return _reservedMudStocks.ContainsKey(guid);
        }
        
        public void ReserveMudStock(string guid, int capacity)
        {
            if (!HasFreePlace())
                return;
            if (IsRegistered(guid))
                return;
            var placeID = _freePlaceIds[0];
            _freePlaceIds.RemoveAt(0);
            _reservedMudStocks.Add(guid, new SimulatingReservedMudStock()
            {
                Guid = guid,
                PlaceNum = placeID,
                Count = 0,
                Capacity = capacity
            });
        }

        public void AddToStock(string guid, int amountToAdd)
        {
            if (!IsRegistered(guid))
                return;

            var reservedMudStock = _reservedMudStocks[guid];
            reservedMudStock.Count = Mathf.Min(reservedMudStock.Count + amountToAdd, reservedMudStock.Capacity);
        }
        public string GetTypeKey()
        {
            return GetType().ToString();
        }

        public void CreateMudStockSceneObjects()
        {
            foreach (var reservedMudStock in _reservedMudStocks.Values)
            {
                
                var createBuildingProtocol = new CreateBuildingProtocol(_mudStockModelID, reservedMudStock.PlaceNum, true, true);

                InventoryDto mudStockInventoryDto = new InventoryDto(createBuildingProtocol.Guid,
                    new[] {new ItemSlot("2", reservedMudStock.Count, reservedMudStock.Capacity)});
                
                _inventoryDtoStorage.Add(mudStockInventoryDto);

                CreateStatsCollectionProtocol createStatsCollectionProtocol =
                    new CreateStatsCollectionProtocol(createBuildingProtocol.Guid,
                        _buildingStatModelStorage.Get(_mudStockModelID).Stats);

                _instantiator.Instantiate<CreateStatsCollectionCommand<BuildingStatsCollection>>()
                    .Execute(createStatsCollectionProtocol);
                
                var roomStatsCollection = _statsCollectionStorage.Get(reservedMudStock.Guid);

                var statCollection = _statsCollectionStorage.Get(createBuildingProtocol.Guid);
                statCollection.AddModifier(_maxUnitsStatKey, new StatModBaseAdd(roomStatsCollection.GetValue(_maxUnitsStatKey)));
                StatVital mudAmountStat = statCollection.Get<StatVital>(_resourceMudStatKey);
                mudAmountStat.CurrentValue = reservedMudStock.Count;
                statCollection.AddModifier(_preProductionMudStatKey, new StatModBaseAdd(1));
                
                _instantiator.Instantiate<CreateBuildingCommand>().Execute(createBuildingProtocol);
            }
        }

        private void OnRoomOpened(OpenedPlaceIDProtocol openedPlaceIDProtocol)
        {
            if(openedPlaceIDProtocol.APlaceModels.Any(aPlace => aPlace == _mudStockModelID))
                _freePlaceIds.Add(openedPlaceIDProtocol.PlaceNum);
        }
        public string Save()
        {
            var freePlacesArray = _placeIdStorage
                .Get(x => x.HasPlace(_mudStockModelID))
                .Where(x => !_reservedPlaceSystem.HasEntity(x.PlaceNumber)).Select(x => x.PlaceNumber).ToArray();
            return JsonHelper.ToJson(freePlacesArray);
        }

        public void Load(string jsonData)
        {
            _freePlaceIds = JsonHelper.FromJson<string>(jsonData).ToList();
        }
        public void Dispose()
        {
            _onRoomOpenedEvent.Dispose();
        }
    }
}