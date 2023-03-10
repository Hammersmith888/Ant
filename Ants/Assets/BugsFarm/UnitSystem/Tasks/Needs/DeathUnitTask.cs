using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.TaskSystem;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class DeathUnitTask : BaseTask
    {
        public override bool Interruptible => false;

        private readonly AnimatorStorage _animatorStorage;
        private readonly UnitSceneObjectStorage _sceneObjectStorage;
        private readonly IInstantiator _instantiator;
        private readonly Action _startFadeAction;

        private UnitSceneObject _unitView;
        private ISpineAnimator _animator;
        private ITask _fadeTask;
        private string _unitGuid;

        public DeathUnitTask(AnimatorStorage animatorStorage,
                              UnitSceneObjectStorage sceneObjectStorage,
                              IInstantiator instantiator,
                              Action startFadeAction)
        {
            _animatorStorage = animatorStorage;
            _sceneObjectStorage = sceneObjectStorage;
            _instantiator = instantiator;
            _startFadeAction = startFadeAction;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned)
            {
                return;
            }

            base.Execute(args);
            _unitGuid = (string) args[0];
            _unitView = _sceneObjectStorage.Get(_unitGuid);
            _animator = _animatorStorage.Get(_unitGuid);
            _animator.OnAnimationComplete += OnAnimationComplete;
            _animator.SetAnim(AnimKey.Death);
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned)  return;
            if (animKey == AnimKey.Death && _fadeTask == null)
            {
                StartFade();
            }
        }

        private void StartFade()
        {
            if (!IsRunned)  return;
            _startFadeAction?.Invoke();
            _fadeTask = _instantiator.Instantiate<FadeInOutUnitTask>();
            _fadeTask.OnComplete += OnFadeEnd;
            _fadeTask.Execute(_unitGuid);
        }

        private void OnFadeEnd(ITask task)
        {
            // alredy is rip
            _unitView.SetInteraction(false);
            _unitView.SetActive(false);
            Completed();
        }

        protected override void OnDisposed()
        {
            _fadeTask?.Interrupt();
            _fadeTask = null;
            _unitView = null;
            _animator = null;
            base.OnDisposed();
        }
    }
}