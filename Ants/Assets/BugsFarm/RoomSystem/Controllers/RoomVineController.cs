using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{
    /// <summary>
    /// Комната с задачами стройка лианы
    /// </summary>
    public class RoomVineController : IInitializable, ISceneEntity
    {
        public string Id { get; private set; }

        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _autoOpenStatKey = "stat_autoOpen";
        private const string _progressPointName = "UIProgressPoint";
        private const string _lockPointName = "RoomLockPoint";
        private const string _stockModelID = "47";  // HerbStock
        private const string _itemID  = "5"; // ReadyHerbItem
        
        private int ActionBuildVine { get; } = 4;   // repeat animation
        private AnimKey WalkAddAnimKey => AnimKey.WalkHerbs;
        private AnimKey[] ActionAnimKeys { get; } = {AnimKey.Build4, AnimKey.Build5};
        private string[] ActionSoundsKeys { get; } = {"RoomDig1", "RoomDig2", "RoomDig3"};
        private Type AddTaskType { get; } = typeof(AddVineBuildTask);

        private IRoomsSystem _roomsSystem;
        private AddResourceSystem _addResourceSystem;
        private IInstantiator _instantiator;
        private IMonoPool _monoPool;
        private StatsCollectionStorage _statsCollectionStorage;
        private RoomSceneObjectStorage _viewStorage;
        private BuildingDtoStorage _buildingDtoStorage;
        private PlaceIdStorage _placeIdStorage;
        private IReservedPlaceSystem _reservedPlaceSystem;
        private UIWorldRoot _uiWorldRoot;

        private IInventory _inventory;
        private IDisposable _placeChangedEvent;
        private StatsCollection _statsCollection;
        private CompositeDisposable _events;
        private UIRoomProgress _uiProgress;
        private RoomLock _roomLock;
        private bool _hasDependency;
        private bool _finalized;

        [Inject]
        private void Inject(string guid,
                            IRoomsSystem roomsSystem,
                            AddResourceSystem addResourceSystem,
                            IInstantiator instantiator,
                            IMonoPool monoPool,
                            StatsCollectionStorage statCollectionStorage,
                            RoomSceneObjectStorage viewStorage,
                            BuildingDtoStorage buildingDtoStorage,
                            PlaceIdStorage placeIdStorage,
                            UIWorldRoot uiWorldRoot,
                            IReservedPlaceSystem reservedPlaceSystem)
        {
            Id = guid;
            _roomsSystem = roomsSystem;
            _addResourceSystem = addResourceSystem;
            _instantiator = instantiator;
            _monoPool = monoPool;
            _statsCollectionStorage = statCollectionStorage;
            _viewStorage = viewStorage;
            _uiWorldRoot = uiWorldRoot;
            _reservedPlaceSystem = reservedPlaceSystem;
            _buildingDtoStorage = buildingDtoStorage;
            _placeIdStorage = placeIdStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            // статы
            _statsCollection = _statsCollectionStorage.Get(Id);
            _hasDependency = _statsCollection.HasEntity(_autoOpenStatKey);

            // инвентарь
            var slot = new ItemSlot(_itemID, (int)_statsCollection.GetVitalValue(_resourceStatKey),
                                             (int)_statsCollection.GetValue(_resourceStatKey));
            var invetoryProtocol = new CreateInventoryProtocol(Id, res => _inventory = res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);
            
            Listen<NextRoomProtocol>(OnRoomNext);
            Listen<OpenableRoomProtocol>(OnRoomOpenable);
            Listen<OpenRoomProtocol>(OnRoomOpened);
            _placeChangedEvent = MessageBroker.Default
                .Receive<PlaceChangedProtocol>()
                .Subscribe(OnPlaceChangedEventHandler);

            SetVisible(false);
            SetIntreractable(false);

            var roomProtocol = new RoomSystemProtocol(Id, IsOpened, true, _hasDependency);
            _roomsSystem.Registration(roomProtocol);
        }

        public void Dispose()
        {
            if (_finalized) return;

            _finalized = true;
            _placeChangedEvent?.Dispose();
            _placeChangedEvent = null;
            _events?.Dispose();
            _events?.Clear();
            _events = null;
            _statsCollection = null;
            
            _reservedPlaceSystem.OnPlaceFree -= OnPlaceFree;
            _addResourceSystem.OnResourceFull -= OnRoomResourceFull;
            _addResourceSystem.OnResourceChanged -= OnRoomResourceChanged;
            _roomsSystem.UnRegistration(Id);
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
        }

        private void Listen<T>(Action<T> onEvent)
        {
            MessageBroker.Default.Receive<T>().Subscribe(onEvent).AddTo(_events);
        }
        
        private void Production()
        {
            if (_finalized) return;

            var opened = IsOpened();
            var stockCount = _buildingDtoStorage.Get().Count(x => x.ModelID == _stockModelID);
            if (_uiProgress && stockCount == 0 && !opened && HasPlaceNum(_stockModelID))
            {
                var buildingBuildProtocol = new CreateBuildingProtocol(_stockModelID, PlaceBuildingUtils.OffScreenPlaceNum, true, true);
                _instantiator.Instantiate<CreateBuildingCommand>().Execute(buildingBuildProtocol);
            }

            if (!opened && !_addResourceSystem.Contains(_itemID, Id))
            {
                var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(ActionAnimKeys)
                    .SetWalkAnimKeys(WalkAddAnimKey)
                    .SetActionSoundKeys(ActionSoundsKeys)
                    .SetRepeatCounts(ActionBuildVine);
                const string fakeModelID = "-1";
                var protocol = new AddResourceProtocol(Id, fakeModelID, _itemID, maxUnits,
                                                       taskPoints, AddTaskType, resourceArgs, true);
                _addResourceSystem.Registration(protocol);
            }
            else if (opened && _addResourceSystem.Contains(_itemID, Id))
            {
                _addResourceSystem.UnRegistration(_itemID, Id);
            }
        }

        private bool HasPlaceNum(string modelId)
        {
            return _placeIdStorage.Get().Any(x=> x.HasPlace(modelId) && !_reservedPlaceSystem.HasEntity(x.PlaceNumber));
        }

        private void SetVisible(bool visible)
        {
            if (_finalized) return;
            if (!_viewStorage.HasEntity(Id))
            {
                return;
                throw new
                    InvalidOperationException($"{this} : {nameof(SetVisible)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);
            view.ChangeVisible(visible);
            view.ChangeIntreaction(visible);
        }

        private void ActivateLock(bool activate)
        {
            if (_finalized) return;
            if (!_viewStorage.HasEntity(Id))
            {
                return;
                throw new
                    InvalidOperationException($"{this} : {nameof(ActivateLock)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);

            if (activate && !IsOpened())
            {
                var locktransform = view.SelfContainer.transform.Find(_lockPointName);
                if (!locktransform || _roomLock)
                {
                    return;
                }

                view.ChangeAlpha(0.35f);
                _roomLock = _monoPool.Spawn<RoomLock>();
                _roomLock.SetPosition(locktransform.position);
                SetVisible(true);
                SetIntreractable(true);
            }
            else
            {
                if (!_roomLock)
                {
                    return;
                }

                view.ChangeAlpha(1f);
                _monoPool.Despawn(_roomLock);
                _roomLock = null;
                SetIntreractable(false);
            }
        }

        private void SetIntreractable(bool interactable)
        {
            if (_finalized) return;
            if (!_viewStorage.HasEntity(Id))
            {
                throw new
                    InvalidOperationException($"{this} : {nameof(ActivateLock)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);
            view.ChangeIntreaction(interactable);
        }

        private void ActivateProgress(bool activate)
        {
            if (_finalized) return;
            if (activate)
            {
                if (!_viewStorage.HasEntity(Id))
                {
                    throw new
                        InvalidOperationException($"{this} : {nameof(ActivateProgress)} :: {nameof(RoomBaseSceneObject)} does not exist.");
                }

                var view = _viewStorage.Get(Id);
                var progressPoint = view.SelfContainer.transform.Find(_progressPointName);
                if (!progressPoint)
                {
                    throw new
                        NullReferenceException($"{this} : {nameof(ActivateProgress)} :: {nameof(Transform)} point does not exist.");
                }

                _uiProgress = _uiProgress ? _uiProgress : _monoPool.Spawn<UIRoomProgress>(_uiWorldRoot.Transform);
                _uiProgress.ChangePosition(progressPoint.position);
                UpdateProgress();
            }
            else
            {
                if (!_uiProgress)
                {
                    return;
                }

                _monoPool.Despawn(_uiProgress);
                _uiProgress = null;
            }
        }

        private bool IsOpened()
        {
            if (_finalized) return false;
            
            var itemSlot = _inventory.GetItemSlot(_itemID);
            return  itemSlot.Count >= itemSlot.Capacity;
        }
        
        private void UpdateProgress()
        {
            if (!_uiProgress || _finalized) return;
            
            var slot = _inventory.GetItemSlot(_itemID);
            _uiProgress.ChangeProgress((float)slot.Count / slot.Capacity);
            _uiProgress.ChangeText(Format.Resource(slot.Count, slot.Capacity));
        }

        // эта комната текущая в очереди на задачи или открывание.
        private void OnRoomNext(NextRoomProtocol protocol)
        {
            if(protocol.Guid != Id || _finalized) return;

            _reservedPlaceSystem.OnPlaceFree += OnPlaceFree;
            _addResourceSystem.OnResourceFull += OnRoomResourceFull;
            _addResourceSystem.OnResourceChanged += OnRoomResourceChanged;

            ActivateProgress(true);
            Production();
        }

        // комната открылась
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if(protocol.Guid != Id || _finalized) return;
            
            SetVisible(true);
            ActivateLock(false);
            ActivateProgress(false);
            SetIntreractable(false);
            var additemsProtocol = new AddItemsProtocol(_itemID,(int)_statsCollection.GetValue(_resourceStatKey), Id);
            _instantiator.Instantiate<AddItemsCommand>().Execute(additemsProtocol);
            Production();
        }

        // Комната еще не открыта но, комнату уже можно окрыть.
        private void OnRoomOpenable(OpenableRoomProtocol protocol)
        {
            if(protocol.Guid != Id || _finalized) return;

            ActivateLock(true);
        }

        // отслеживаю состояния ресурса в комнате
        private void OnRoomResourceFull(string itemID, string guid)
        {
            if(_finalized) return;
            if (itemID != _itemID || guid != Id || !_uiProgress)
            {
                return;
            }

            if (!_addResourceSystem.Contains(_itemID, Id))
            {
                return;
            }

            _reservedPlaceSystem.OnPlaceFree -= OnPlaceFree;
            _addResourceSystem.OnResourceFull -= OnRoomResourceFull;
            _addResourceSystem.OnResourceChanged -= OnRoomResourceChanged;

            _addResourceSystem.UnRegistration(_itemID, guid);
            _roomsSystem.OpenRoom(Id);
        }

        // ресурс в комнате изменился
        private void OnRoomResourceChanged(string itemID, string guid)
        {
            if(_finalized) return;
            if (itemID != _itemID || guid != Id || !_uiProgress)
            {
                return;
            }

            UpdateProgress();
        }

        private void OnPlaceFree(string placeNum)
        {
            if(_finalized) return;
            if (!_uiProgress || IsOpened())
            {
                return;
            }

            Production();
        }

        private void OnPlaceChangedEventHandler(PlaceChangedProtocol protocol)
        {
            if(_finalized || !_uiProgress || IsOpened()) return;
            
            Production();
        }
    }
}