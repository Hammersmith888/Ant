using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.TaskSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class TrainingTask : BaseTrainingTask
    {
        protected virtual float DelaySecondsBetwenTrain => 0.1f;
        private AudioModelStorage _audioModelStorage;
        private SceneEntityStorage _sceneEntityController;
        private AnimatorStorage _animatorStorage;
        private ISoundSystem _soundSystem;
        private IInstantiator _instantiator;

        protected virtual AnimKey[] Anims { get; } = {AnimKey.Attack, AnimKey.Attack2, AnimKey.Training};
        protected virtual string[] Sounds { get; } = {"Attack_1", "Attack_2", "Training"};

        protected ISpineAnimator Animator;
        protected AnimKey CurrentTrain;
        private string _currentSound;
        private ITask _timerBetwenTrainTask;

        [Inject]
        private void Inject(ISoundSystem soundSystem,
                            SceneEntityStorage entityController,
                            AudioModelStorage audioModelStorage,
                            AnimatorStorage animatorStorage,
                            IInstantiator instantiator)
        {
            _soundSystem = soundSystem;
            _audioModelStorage = audioModelStorage;
            _sceneEntityController = entityController;
            _animatorStorage = animatorStorage;
            _instantiator = instantiator;
        }
        protected override void ExecuteInheritor(params object[] args)
        {
            var unitGuid = (string) args[0];
            var entity = _sceneEntityController.Get(unitGuid);
            var unitAudioModel = _audioModelStorage.Get(entity.GetType().Name);

            var index = Random.Range(0, Anims.Length);
            var soundKey = Sounds[index];
            Animator = _animatorStorage.Get(unitGuid);
            Animator.OnAnimationComplete += OnAnimationComplete;
            if (unitAudioModel.HasAudioClip(soundKey))
            {
                _currentSound = unitAudioModel.GetAudioClip(Sounds[index]);
            }
            CurrentTrain = Anims[index];
        }

        // не вызывается при цикличных анимациях, такие анимации заканчиваются со временем.
        private void OnAnimationComplete(AnimKey animKey)
        {
            if(SimulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted || animKey != CurrentTrain) return;
            
            if (_timerBetwenTrainTask == null)
            {
                _timerBetwenTrainTask = _instantiator.Instantiate<SimulatedTimerTask>();
                _timerBetwenTrainTask.OnComplete += OnTimerBetwenTrainComplete;
                _timerBetwenTrainTask.Execute(DelaySecondsBetwenTrain);
            }
            else
            {
                Animator.SetAnim(AnimKey.Idle);
            }
        }

        private void OnTimerBetwenTrainComplete(ITask task)
        {
            if(SimulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted) return;
            OnTrain();
        }
        
        protected override void OnTrain()
        {
            if(SimulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted) return;
            _timerBetwenTrainTask?.Interrupt();
            _timerBetwenTrainTask = null;
            Animator.SetAnim(CurrentTrain);
            if(!string.IsNullOrEmpty(_currentSound))
            {
                _soundSystem.Play(SceneObject.transform.position, _currentSound);
            }
        }

        protected override void OnRest()
        {
            if(SimulationSystem.Simulation) return;
            if (!IsRunned || IsCompleted) return;
            _timerBetwenTrainTask?.Interrupt();
            _timerBetwenTrainTask = null;
            Animator.SetAnim(AnimKey.Idle);
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                Animator.OnAnimationComplete -= OnAnimationComplete;
                OnRest();
            }

            _timerBetwenTrainTask?.Interrupt();
            _timerBetwenTrainTask = null;
            Animator = null;
            _currentSound = null;
            _audioModelStorage = null;
            _sceneEntityController = null;
            _soundSystem = null;
            base.OnDisposed();
        }
    }
}