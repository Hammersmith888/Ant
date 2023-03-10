using System;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using UniRx;
using Zenject;

namespace BugsFarm.RoomSystem
{
    // комната : может быть открыта по умолчанию, может открытся зависимо автоматически.
    public class RoomBaseController : ISceneEntity, IInitializable
    {
        public string Id { get; private set; }
        private IRoomsSystem _roomsSystem;
        private RoomSceneObjectStorage _viewStorage;
        private StatsCollectionStorage _statsCollectionStorage;

        private const string _progressStatKey = "stat_progress";
        private const string _autoOpenStatKey = "stat_autoOpen";

        private CompositeDisposable _events;
        private StatsCollection _statsCollection;
        private bool _hasDependency;

        [Inject]
        private void Inject(string guid,
                            IRoomsSystem roomsSystem,
                            RoomSceneObjectStorage viewStorage,
                            StatsCollectionStorage statCollectionStorage)
        {
            Id = guid;
            _roomsSystem = roomsSystem;
            _viewStorage = viewStorage;
            _statsCollectionStorage = statCollectionStorage;
            _events = new CompositeDisposable();
        }

        public void Initialize()
        {
            // статы
            _statsCollection = _statsCollectionStorage.Get(Id);
            _hasDependency = _statsCollection.HasEntity(_autoOpenStatKey);
            MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(p => OnRoomOpened(p.Guid)).AddTo(_events);
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

        protected void SetVisible(bool visible)
        {
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

        private bool IsOpened()
        {
            return _statsCollection.GetValue(_progressStatKey) > 0;
        }

        // комната открылась
        protected virtual void OnRoomOpened(string guid)
        {
            if (guid != Id)
            {
                return;
            }

            _statsCollection.AddModifier(_progressStatKey, new StatModBaseAdd(1));
            SetVisible(false);
        }
    }
}