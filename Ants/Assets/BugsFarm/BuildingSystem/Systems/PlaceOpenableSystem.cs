using System;
using System.Collections.Generic;
using BugsFarm.AssetLoaderSystem;
using BugsFarm.RoomSystem;
using UniRx;
using UnityEngine;
using Zenject;
using Object = UnityEngine.Object;

namespace BugsFarm.BuildingSystem
{
    public class PlaceOpenableSystem : IPlaceOpenableSystem, IInitializable, IDisposable
    {
        private readonly IInstantiator _instantiator;
        private readonly IRoomsSystem _roomsSystem;
        private readonly PrefabLoader _prefabLoader;
        private readonly RoomDtoStorage _roomDtoStorage;
        private readonly Dictionary<string, GameObject> _placeIdGroupes;
        private readonly Transform _placeIdsParent;
        private const string _prefix = "PlaceIDGroup_";
        private IDisposable _openRoomEvent;
        
        public PlaceOpenableSystem(IInstantiator instantiator, 
                                   IRoomsSystem roomsSystem,
                                   PrefabLoader prefabLoader,
                                   RoomDtoStorage roomDtoStorage)
        {
            _instantiator = instantiator;
            _roomsSystem = roomsSystem;
            _prefabLoader = prefabLoader;
            _roomDtoStorage = roomDtoStorage;
            _placeIdGroupes = new Dictionary<string, GameObject>();
            _placeIdsParent = new GameObject("PlaceIDGroups").transform;
        }
        
        void IInitializable.Initialize()
        {
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
        }

        void IDisposable.Dispose()
        {
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
            Reset();
            if (_placeIdsParent)
            {
                Object.Destroy(_placeIdsParent.gameObject);
            }
        }

        public void Reset()
        {
            var placeGroups = _placeIdGroupes.Values;
            foreach (var placeGroup in placeGroups)
            {
                if(!placeGroup) continue;
                Object.Destroy(placeGroup);
            }
            _placeIdGroupes.Clear();
        }
        
        public void OpenGroupe(string groupe)
        {
            if (!_placeIdGroupes.ContainsKey(groupe))
            {
                var placePrefab = _prefabLoader.Load(_prefix + groupe);
                if (!placePrefab)
                {
                    Debug.LogError($"{this} OpenGroupe :: PlaceIDGroupe with [GroupID : {groupe}] , does not exist");
                    _placeIdGroupes.Add(groupe, null);
                    return;
                }
                var placeGroupe = _instantiator.InstantiatePrefab(placePrefab,_placeIdsParent);
                _placeIdGroupes.Add(groupe, placeGroupe);
                MessageBroker.Default.Publish(new PlaceChangedProtocol(groupe));
            }
        }
        
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (!_roomDtoStorage.HasEntity(protocol.Guid))
            {
                throw new InvalidOperationException($"{nameof(RoomDto)} with [Guid : {protocol.Guid}], does not exist.");
            }

            var dto = _roomDtoStorage.Get(protocol.Guid);
            OpenGroupe(dto.ModelID);
        }
    }
}