using System;
using BugsFarm.ReloadSystem;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.RoomSystem
{
    // Комната : может быть открыта по умолчанию, может быть куплена, может открытся зависимо автоматически.
    public class RoomOpenableController : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IRoomsSystem _roomsSystem;
        private readonly RoomSceneObjectStorage _viewStorage;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly IMonoPool _monoPool;
        
        private const string _progressStatKey = "stat_progress";
        private const string _autoOpenStatKey = "stat_autoOpen";
        private const string _lockPointName = "RoomLockPoint";

        private StatsCollection _statsCollection;
        private CompositeDisposable _events;
        private RoomLock _roomLock;
        private bool _hasDependency;
        
        public RoomOpenableController(string guid,
                                      IRoomsSystem roomsSystem,
                                      IMonoPool monoPool,
                                      RoomSceneObjectStorage viewStorage,
                                      StatsCollectionStorage statsCollectionStorage)
        {
            Id = guid;
            _roomsSystem = roomsSystem;
            _monoPool = monoPool;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statsCollectionStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            // статы
            _statsCollection = _statsCollectionStorage.Get(Id);
            _hasDependency = _statsCollection.HasEntity(_autoOpenStatKey);
            
            Listen<OpenableRoomProtocol>(OnRoomOpenable);
            Listen<OpenRoomProtocol>(OnRoomOpened);
            Listen<GameReloadingReport>(OnGameReloading);
            SetIntreractable(false);

            var roomProtocol = new RoomSystemProtocol(Id, IsOpened, false, _hasDependency);
            _roomsSystem.Registration(roomProtocol);
        }

        public void Dispose()
        {
            _statsCollection = null;
            _events?.Dispose();
            _events?.Clear();
            _events = null;
            _roomsSystem.UnRegistration(Id);
        }
        private void OnGameReloading(GameReloadingReport reloadingReport)
        {
            if (_roomLock != null)
            {
                _monoPool.Despawn(_roomLock);
                SetIntreractable(false);
            }
        }

        private void Listen<T>(Action<T> onEvent)
        {
            MessageBroker.Default.Receive<T>().Subscribe(onEvent).AddTo(_events);
        }
        
        private void SetVisible(bool visible)
        {
            if (!_viewStorage.HasEntity(Id))
            {
                throw new InvalidOperationException($"{this} : {nameof(SetVisible)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);
            view.ChangeVisible(visible);
            view.ChangeIntreaction(visible);
        }

        private void ActivateLock(bool activate)
        {
            if (activate && !IsOpened())
            {
                if (!_viewStorage.HasEntity(Id))
                {
                    throw new InvalidOperationException($"{this} : {nameof(ActivateLock)} :: {nameof(RoomBaseSceneObject)} does not exist.");
                }

                var view = _viewStorage.Get(Id);
                var locktransform = view.SelfContainer.transform.Find(_lockPointName);

                if (!locktransform || _roomLock)
                {
                    return;
                }
                    
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
                throw new InvalidOperationException($"{this} : {nameof(ActivateLock)} :: {nameof(RoomBaseSceneObject)} does not exist.");
            }

            var view = _viewStorage.Get(Id);
            view.ChangeIntreaction(interactable);
        }

        private bool IsOpened()
        {
            return _statsCollection.GetValue(_progressStatKey) > 0;
        }

        // комната открылась
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (protocol.Guid != Id)
            {
                return;
            }

            _statsCollection.AddModifier(_progressStatKey, new StatModBaseAdd(1));
            SetVisible(false);
            ActivateLock(false);
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
    }
}