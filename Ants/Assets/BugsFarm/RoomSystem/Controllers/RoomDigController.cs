using System;
using System.Linq;
using BugsFarm.AnimationsSystem;
using BugsFarm.BuildingSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.InventorySystem;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.CommandService;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using BugsFarm.Services.UIService;
using BugsFarm.TaskSystem;
using BugsFarm.UI;
using UniRx;
using UnityEngine;
using Zenject;

namespace BugsFarm.RoomSystem
{
    /// <summary>
    /// Комната с задачами копать
    /// </summary>
    public class RoomDigController : IInitializable, ISceneEntity
    {
        public string Id { get; private set; }
        protected virtual AnimKey ActionAnimKey => AnimKey.DigMidle;
        private string ItemID => "2"; // MudItem
        private int DigCount => 7;

        private readonly Type _taskType = typeof(GetRoomDigTask);
        private string[] Sounds { get; } = {"RoomDig1", "RoomDig2", "RoomDig3"};

        private const string _resourceStatKey = "stat_maxResource";
        private const string _maxUnitsStatKey = "stat_maxUnits";
        private const string _resourceMudStatKey = "stat_maxResource";
        private const string _preProductionMudStatKey = "stat_preProduction";
        private const string _autoOpenStatKey = "stat_autoOpen";
        private const string _progressPointName = "UIProgressPoint";
        private const string _lockPointName = "RoomLockPoint";
        private const string _mudStockModelID = "49";

        private IRoomsSystem _roomsSystem;
        private GetResourceSystem _getResourceSystem;
        private AddResourceSystem _addResourceSystem;
        private IInstantiator _instantiator;
        private IMonoPool _monoPool;
        private StatsCollectionStorage _statsCollectionStorage;
        private InventoryDtoStorage _inventoryDtoStorage;
        private RoomSceneObjectStorage _viewStorage;
        private BuildingDtoStorage _buildingDtoStorage;
        private PlaceIdStorage _placeIdStorage;
        private IReservedPlaceSystem _reservedPlaceSystem;
        
        private UIWorldRoot _uiWorldRoot;
        private IInventory _inventory;
        private StatsCollection _statsCollection;
        private CompositeDisposable _events;
        private UIRoomProgress _uiProgress;
        private RoomLock _roomLock;
        private bool _hasDependency;
        private bool _finalized;

        [Inject]
        private void Inject(string guid,
                            IRoomsSystem roomsSystem,
                            GetResourceSystem getResourceSystem,
                            AddResourceSystem addResourceSystem,
                            TaskStorage taskStorage,
                            IInstantiator instantiator,
                            IMonoPool monoPool,
                            StatsCollectionStorage statsCollectionStorage,
                            InventoryDtoStorage inventoryDtoStorage,
                            RoomSceneObjectStorage viewStorage,
                            BuildingDtoStorage buildingDtoStorage,
                            PlaceIdStorage placeIdStorage,
                            UIWorldRoot uiWorldRoot,
                            IReservedPlaceSystem reservedPlaceSystem)
        {
            Id = guid;
            _roomsSystem = roomsSystem;
            _getResourceSystem = getResourceSystem;
            _addResourceSystem = addResourceSystem;
            _instantiator = instantiator;
            _monoPool = monoPool;
            _statsCollectionStorage = statsCollectionStorage;
            _viewStorage = viewStorage;
            _reservedPlaceSystem = reservedPlaceSystem;
            _uiWorldRoot = uiWorldRoot;
            _placeIdStorage = placeIdStorage;
            _buildingDtoStorage = buildingDtoStorage;
            _inventoryDtoStorage = inventoryDtoStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            // статы
            _statsCollection = _statsCollectionStorage.Get(Id);
            _hasDependency = _statsCollection.HasEntity(_autoOpenStatKey);
            // инвентарь
            var slot = new ItemSlot(ItemID, (int)_statsCollection.GetVitalValue(_resourceStatKey), 
                                            (int)_statsCollection.GetValue(_resourceStatKey));
            var invetoryProtocol = new CreateInventoryProtocol(Id, res => _inventory = res, slot);
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(invetoryProtocol);

            Listen<NextRoomProtocol>(OnRoomNext);
            Listen<OpenableRoomProtocol>(OnRoomOpenable);
            Listen<OpenRoomProtocol>(OnRoomOpened);
            Listen<GameReloadingReport>(OnGameReloading);

            SetIntreractable(false);

            var roomProtocol = new RoomSystemProtocol(Id, IsOpened, true, _hasDependency);
            _roomsSystem.Registration(roomProtocol);
        }

        private void OnGameReloading(GameReloadingReport reloadingReport)
        {
            if (_roomLock != null)
            {
                _monoPool.Despawn(_roomLock);
            }

            if (_uiProgress != null)
            {
                _monoPool.Despawn(_uiProgress);
            }
        }

        public void Dispose()
        {
            if (_finalized)
            {
                return;
            }

            _finalized = true;
            _statsCollection = null;
            _events?.Dispose();
            _events?.Clear();
            _events = null;
            
            _reservedPlaceSystem.OnPlaceFree -= OnPlaceFree;
            _getResourceSystem.OnResourceDepleted -= OnRoomResourceDepleted;
            _getResourceSystem.OnResourceChanged -= OnRoomResourceChanged;
            _addResourceSystem.OnSystemUpdate -= OnAddSystemUpdate;

            _getResourceSystem.UnRegistration(ItemID, Id);
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
            var validStock = _uiProgress && _buildingDtoStorage.Get().Any(x =>
            {
                if (x.ModelID != _mudStockModelID) return false;
                var slots = _inventoryDtoStorage.Get(x.Guid).Slots.ToArray();
                var targetSlot = slots.First(slot => slot.ItemID == ItemID);
                return x.PlaceNum == PlaceBuildingUtils.OffScreenPlaceNum || targetSlot.Count < targetSlot.Capacity;
            });
            
            if (_uiProgress && !opened && !validStock && HasPlaceNum(_mudStockModelID))
            {
                var createBuildingProtocol = new CreateBuildingProtocol(_mudStockModelID, PlaceBuildingUtils.OffScreenPlaceNum, true, true);
                _instantiator.Instantiate<CreateBuildingCommand>().Execute(createBuildingProtocol);

                var statCollection = _statsCollectionStorage.Get(createBuildingProtocol.Guid);
                statCollection.AddModifier(_maxUnitsStatKey, new StatModBaseAdd(_statsCollection.GetValue(_maxUnitsStatKey)));
                statCollection.AddModifier(_resourceMudStatKey, new StatModBaseAdd(_statsCollection.GetValue(_resourceStatKey)));
                statCollection.AddModifier(_preProductionMudStatKey, new StatModBaseAdd(1));
            }

            if (!opened && !_getResourceSystem.Contains(ItemID, Id))
            {
                var taskPoints = _viewStorage.Get(Id).GetComponent<TasksPoints>().Points;
                var maxUnits = (int)_statsCollection.GetValue(_maxUnitsStatKey);
                var resourceArgs = ResourceArgs.Default()
                    .SetActionAnimKeys(ActionAnimKey)
                    .SetActionSoundKeys(Sounds)
                    .SetRepeatCounts(DigCount);
                const string fakeModelID = "-1";
                var protocol = new GetResourceProtocol(Id, fakeModelID, ItemID, maxUnits, taskPoints,
                                                       _taskType, resourceArgs);
                _getResourceSystem.Registration(protocol);
            }
            else if (opened && _getResourceSystem.Contains(ItemID, Id))
            {
                _getResourceSystem.UnRegistration(ItemID, Id);
            }
        }

        private bool HasPlaceNum(string modelId)
        {
            return _placeIdStorage.Get().Any(x=> x.HasPlace(modelId) && !_reservedPlaceSystem.HasEntity(x.PlaceNumber));
        }

        private void SetVisible(bool visible)
        {
            if (!_viewStorage.HasEntity(Id))
            {
                return;
                throw new
                    InvalidOperationException($"{this} : {nameof(SetVisible)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);
            view.ChangeVisible(visible);
        }

        private void ActivateLock(bool activate)
        {
            if (activate && !IsOpened())
            {
                if (!_viewStorage.HasEntity(Id))
                {
                    return;
                    throw new
                        InvalidOperationException($"{this} : {nameof(ActivateLock)} :: {nameof(RoomBaseSceneObject)} does not exist.");
                }

                var view = _viewStorage.Get(Id);
                var locktransform = view.SelfContainer.transform.Find(_lockPointName);
                if (!locktransform || _roomLock) return;

                _roomLock = _monoPool.Spawn<RoomLock>();
                _roomLock.SetPosition(locktransform.position);
                SetIntreractable(true);
            }
            else
            {
                if (!_roomLock)
                {
                    return;
                }

                _monoPool.Despawn(_roomLock);
                _roomLock = null;
                SetIntreractable(false);
            }
        }

        private void SetIntreractable(bool interactable)
        {
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
            if (activate)
            {
                if (!_viewStorage.HasEntity(Id))
                {
                    return;
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
            return !_inventory.HasItem(ItemID);
        }

        private void UpdateProgress()
        {
            if (!_uiProgress || _finalized) return;
            
            var slot = _inventory.GetItemSlot(ItemID);
            _uiProgress.ChangeProgress(1f - ((float)slot.Count / slot.Capacity));
            _uiProgress.ChangeText(Format.Resource(slot.Capacity - slot.Count, slot.Capacity));
        }

        // эта комната в очереди на задачи.
        private void OnRoomNext(NextRoomProtocol protocol)
        {
            if (protocol.Guid != Id)
            {
                return;
            }

            _reservedPlaceSystem.OnPlaceFree += OnPlaceFree;
            _getResourceSystem.OnResourceDepleted += OnRoomResourceDepleted;
            _getResourceSystem.OnResourceChanged += OnRoomResourceChanged;
            _addResourceSystem.OnSystemUpdate += OnAddSystemUpdate;
            
            ActivateProgress(true);
            Production();
        }

        // комната открылась
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (protocol.Guid != Id)
            {
                return;
            }
            
            SetVisible(false);
            ActivateLock(false);
            ActivateProgress(false);
            SetIntreractable(false);
            _inventory.Remove(ItemID);
            Production();
        }

        // Комната еще не открыта но, комнату уже можно окрыть.
        private void OnRoomOpenable(OpenableRoomProtocol protocol)
        {
            if (protocol.Guid != Id)
            {
                return;
            }

            ActivateLock(true);
        }

        // отслеживаю состояния ресурса в комнате
        private void OnRoomResourceDepleted(string itemID, string guid)
        {
            if (itemID != ItemID || guid != Id || !_uiProgress)
            {
                return;
            }

            _reservedPlaceSystem.OnPlaceFree -= OnPlaceFree;
            _getResourceSystem.OnResourceDepleted -= OnRoomResourceDepleted;
            _getResourceSystem.OnResourceChanged -= OnRoomResourceChanged;
            _addResourceSystem.OnSystemUpdate -= OnAddSystemUpdate;

            _roomsSystem.OpenRoom(Id);
            Production();
        }

        // ресурс в комнате изменился
        private void OnRoomResourceChanged(string itemID, string guid)
        {
            if (itemID != ItemID || guid != Id || !_uiProgress)
            {
                return;
            }

            UpdateProgress();
        }

        // если ресурс пропал / появился на сцене
        private void OnAddSystemUpdate(string itemID, string guid)
        {
            if (ItemID != itemID || !_uiProgress)
            {
                return;
            }

            Production();
        }

        private void OnPlaceFree(string placeNum)
        {
            if (!_uiProgress || IsOpened())
            {
                return;
            }

            Production();
        }
    }
}