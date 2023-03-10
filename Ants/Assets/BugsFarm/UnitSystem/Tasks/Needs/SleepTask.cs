using BugsFarm.AnimationsSystem;
using BugsFarm.InfoCollectorSystem;
using BugsFarm.ReloadSystem;
using BugsFarm.SimulationSystem;
using BugsFarm.SpeakerSystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class SleepTask : BaseTask, ITickable
    {
        private readonly NeedStatController _needController;
        private readonly ITickableManager _tickableManager;
        private readonly ISimulationSystem _simulationSystem;
        private readonly ISpeakerSystem _speakerSystem;
        private readonly AnimatorStorage _animatorStorage;
        private ISpineAnimator _animator;
        private string _entityId;

        public SleepTask(NeedStatController needController,
                         ITickableManager tickableManager,
                         ISimulationSystem simulationSystem,
                         ISpeakerSystem speakerSystem,
                         AnimatorStorage animatorStorage)
        {
            _needController = needController;
            _tickableManager = tickableManager;
            _simulationSystem = simulationSystem;
            _speakerSystem = speakerSystem;
            _animatorStorage = animatorStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }
            base.Execute(args);
            _entityId = (string) args[0];
            _animator = _animatorStorage.Get(_entityId);
            _animator.OnAnimationComplete += OnAnimationComplete;
            _animator.SetAnim(AnimKey.Sleep);
            _speakerSystem.AllowSay(_entityId, false);
            _tickableManager.Add(this);
        }

        public void Tick()
        {
            if (!IsRunned)
            {
                return;
            }
            
            if (_needController.NeedCount > 0)
            {
                _needController.Update(Format.SecondsToMinutes(_simulationSystem.DeltaTime));
            }
            else
            {
                _tickableManager.Remove(this);
                _animator.SetAnim(AnimKey.Awake);
            }
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (animKey == AnimKey.Awake)
            {
                Completed();
            }

            if (animKey == AnimKey.Sleep)
            {
                _needController.RestockStart();
            }
        }

        protected override void OnDisposed()
        {
            if (IsExecuted && !GameReloader.IsReloading)
            {
                _speakerSystem.AllowSay(_entityId, true);
                _speakerSystem.Say(_entityId, PhraseState.awaken);
                _animator.OnAnimationComplete -= OnAnimationComplete;
                _needController.Update(_needController.NeedCount);
            }
            _needController.RestockEnd();
            _animator = null;
            base.OnDisposed();
        }

        protected override void OnInterrupted()
        {
            if (IsExecuted)
            {
                _animator.SetAnim(AnimKey.Awake);
            }
            base.OnInterrupted();
        }
    }
}