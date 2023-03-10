using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.RoomSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StatsService;
using DG.Tweening;
using UniRx;
using Zenject;

namespace BugsFarm.LeafHeapSystem.Controllers
{
    public class LeafHeap : ISceneEntity, IInitializable
    {
        public string Id { get; }
        private readonly IInstantiator _instantiator;
        private readonly StatsCollectionStorage _statsCollectionStorage;
        private readonly RoomDtoStorage _roomDtoStorage;
        private readonly LeafHeapDtoStorage _leafHeapDtoStorage;
        private readonly LeafHeapSceneObjectStorage _viewStorage;

        private const string _contentShowedStatKey = "stat_contentShowed";
        private const string _delayAnimationStatKey = "stat_delayAnimation";
        private const AnimKey _heapAwake = AnimKey.Awake;
        private StatsCollection _statsCollection;
        private LeafHeapSceneObject _view;
        private ISpineAnimator _animator;
        private LeafHeapDto _selfDto;
        private IDisposable _openRoomEvent;
        
        private bool _userSeeTheHeap;
        private bool _resolved;
        private bool _finalized;
        public LeafHeap(string guid,
                        IInstantiator instantiator,
                        StatsCollectionStorage statsCollectionStorage,
                        RoomDtoStorage roomDtoStorage,
                        LeafHeapDtoStorage leafHeapDtoStorage,
                        LeafHeapSceneObjectStorage viewStorage)
        {
            _instantiator = instantiator;
            _statsCollectionStorage = statsCollectionStorage;
            _roomDtoStorage = roomDtoStorage;
            _leafHeapDtoStorage = leafHeapDtoStorage;
            _viewStorage = viewStorage;
            Id = guid;
        }
        
        public void Initialize()
        {
            _statsCollection  = _statsCollectionStorage.Get(Id);
            _view = _viewStorage.Get(Id);
            
            var createAnimatorProtocol = new CreateAnimatorProtocol(Id, GetType().Name,res => _animator = res, _view.MainSkeleton);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(createAnimatorProtocol);

            _selfDto = _leafHeapDtoStorage.Get(Id);
            _openRoomEvent = MessageBroker.Default.Receive<OpenRoomProtocol>().Subscribe(OnRoomOpened);
            _animator.OnAnimationComplete  += OnAnimationComplete;
            _view.VisibleTrigger.OnVisible += OnHeapVisible;
            _view.ChangeVisible(true);
        }
        
        public void Dispose()
        {
            if(_finalized) return;

            if (_animator != null)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
                var removeAnimatorProtocol = new RemoveAnimatorProtocol(Id);
                _instantiator.Instantiate<RemoveAnimatorCommand>().Execute(removeAnimatorProtocol);
                _animator = null;
            }
            
            if (_view)
            {
                _view.VisibleTrigger.OnVisible -= OnHeapVisible;    
            }

            _statsCollection = null;
            _openRoomEvent?.Dispose();
            _openRoomEvent = null;
            _finalized = true;
        }
        
        private void Production()
        {
            if (_finalized)
            {
                return;
            }

            if (_statsCollection.GetValue(_contentShowedStatKey) == 0 && _userSeeTheHeap && _resolved)
            {
                _statsCollection.AddModifier(_contentShowedStatKey, new StatModBaseAdd(1));
                DOVirtual.DelayedCall(_statsCollection.GetValue(_delayAnimationStatKey), () => _animator.SetAnim(_heapAwake)); 
            }
        }
        private void OnAnimationComplete(AnimKey animKey)
        {
            if (_finalized || animKey != _heapAwake)
            {
                return;
            }
            _view.ChangeVisible(false);
            
            var deleteProtocol = new DeleteLeafHeapProtocol(Id);
            _instantiator.Instantiate<DeleteLeafHeapCommand>().Execute(deleteProtocol);
        }
        private void OnRoomOpened(OpenRoomProtocol protocol)
        {
            if (_finalized || !_roomDtoStorage.HasEntity(protocol.Guid))
            {
                return;
            }

            var roomDto = _roomDtoStorage.Get(protocol.Guid);
            if (roomDto.ModelID == _selfDto.ModelID)
            {
                _resolved = true;
                Production();
            }
        }
        private void OnHeapVisible(bool visible)
        {
            if (_finalized)
            {
                return;
            }
            _userSeeTheHeap = visible;
            Production();
        }
    }
}