using BugsFarm.AnimationsSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.SpeakerSystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class Dealer : ISceneEntity, IInitializable
    {
        public string Id { get; }

        private readonly IInstantiator _instantiator;
        private readonly ISpeakerSystem _speakerSystem;
        private readonly IUnitFallSystem _unitFallSystem;
        private readonly IActivitySystem _activitySystem;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private UnitTaskProcessor _taskProcessor;
        private UnitSceneObject _view;
        private bool _finalized;

        protected Dealer(string guid,
                         IInstantiator instantiator,
                         ISpeakerSystem speakerSystem,
                         IActivitySystem activitySystem,
                         IUnitFallSystem unitFallSystem,
                         UnitSceneObjectStorage unitSceneObjectStorage)
        {
            Id = guid;
            _instantiator = instantiator;
            _speakerSystem = speakerSystem;
            _activitySystem = activitySystem;
            _unitFallSystem = unitFallSystem;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }

        public virtual void Initialize()
        {
            if (_finalized) return;
            
            _view = _unitSceneObjectStorage.Get(Id);
            var inventoryProtocol = new CreateInventoryProtocol(Id, res => res.SetDefaultCapacity(1));
            _instantiator.Instantiate<CreateInventoryCommand>().Execute(inventoryProtocol);

            var animatorProtocol = new CreateAnimatorProtocol(Id, GetType().Name, _view.MainSkeleton);
            _instantiator.Instantiate<CreateAnimatorCommand<SpineAnimator>>().Execute(animatorProtocol);
            _instantiator.Instantiate<CreateMoverCommand<UnitMover>>().Execute(new CreateMoverProtocol(Id));
            
            _taskProcessor = _instantiator.Instantiate<UnitTaskProcessor>(new object[] {Id});
            _taskProcessor.Stop();
            _taskProcessor.Initialize();

            var activityProtocol = new ActivitySystemProtocol(Id, Play, Stop);
            _activitySystem.Registration(activityProtocol);
            if (_activitySystem.IsActive(Id))
            {
                _activitySystem.Activate(Id,true,true);
            }
            else
            {
                _view.SetActive(false);
            }
        }

        public virtual void Dispose()
        {
            if (_finalized) return;
            _activitySystem.UnRegistration(Id);
            Stop();
            _taskProcessor.Dispose();
            _instantiator.Instantiate<DeleteInventoryCommand>().Execute(new DeleteInventoryProtocol(Id));
            _finalized = true;
        }

        private void Play()
        {
            if (_finalized) return;

            _view.SetActive(true);
            _taskProcessor.OnFree += Update;
            _taskProcessor.Play();
            _unitFallSystem.Registration(Id);
            _speakerSystem.Registration(Id, GetType().Name, _unitSceneObjectStorage.Get(Id).transform);
            _speakerSystem.ChangeState(Id, PhraseState.idle);
            Update(null);
        }

        private void Stop()
        {
            if (_finalized) return;
            if (_view != null)
                _view.SetActive(false);
            _taskProcessor.OnFree -= Update;
            _taskProcessor.Stop();
            _taskProcessor.Interrupt();
            _unitFallSystem.UnRegistration(Id);
            _speakerSystem.UnRegistration(Id);
        }
        
        private void Update(ITask taskEnd)
        {
            if (_finalized || !_activitySystem.IsActive(Id))
            {
                return;
            }

            if (_taskProcessor.IsFree)
            {
                if (!_taskProcessor.TryStartInterrupted())
                {
                    _taskProcessor.Update();
                    if (_taskProcessor.IsFree)
                    {
                        _activitySystem.Activate(Id, false);
                    }
                }
            }
        }
    }
}