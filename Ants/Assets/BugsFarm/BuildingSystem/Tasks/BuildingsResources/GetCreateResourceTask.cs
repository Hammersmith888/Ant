using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class GetCreateResourceTask : BaseTask
    {
        private readonly string _itemId;
        private readonly ResourceArgs _args;
        private readonly AnimatorStorage _animatorStorage;
        private readonly SceneEntityStorage _entityController;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly ISoundSystem _soundSystem;
        private readonly IInstantiator _instantiator;

        private ISpineAnimator _animator;
        private AudioModel _audioModel;
        private int _repeatCount;
        private int _resourceCount;
        private string _unitGuid;
        private string _soundActionKey;
        private AnimKey _currentAnimKey;
        private UnitSceneObject _sceneObject;

        public GetCreateResourceTask(string itemId,
                                     ResourceArgs args,
                                     AnimatorStorage animatorStorage,
                                     SceneEntityStorage entityController,
                                     AudioModelStorage audioModelStorage,
                                     UnitSceneObjectStorage unitSceneObjectStorage,
                                     ISoundSystem soundSystem,
                                     IInstantiator instantiator)
        {
            _itemId = itemId;
            _args = args;
            _animatorStorage = animatorStorage;
            _soundSystem = soundSystem;
            _instantiator = instantiator;
            _entityController = entityController;
            _audioModelStorage = audioModelStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);
            _unitGuid      = (string) args[0];
            _resourceCount = (int) args[1];

            var entity   = _entityController.Get(_unitGuid);
            _sceneObject = _unitSceneObjectStorage.Get(_unitGuid);
            _animator    = _animatorStorage.Get(_unitGuid);
            _repeatCount = _args.RepeatCount;
            
            var sounds = _args.ActionSoundKeys;
            if (sounds.Length > 0)
            {
                _audioModel = _audioModelStorage.Get(entity.GetType().Name);
                _soundActionKey = sounds[Random.Range(0, sounds.Length)];
            }

            _animator.OnAnimationComplete += OnAnimationComplete;
            SetAnimation();
        }

        private void SetAnimation()
        {
            _repeatCount--;
            _currentAnimKey = _args.ActionAnimKeys[Random.Range(0, _args.ActionAnimKeys.Length)];
            _animator.SetAnim(_currentAnimKey);

            if (!string.IsNullOrEmpty(_soundActionKey) && _audioModel != null &&
                _audioModel.HasAudioClip(_soundActionKey))
            {
                _soundSystem.Play(_sceneObject.transform.position,_audioModel.GetAudioClip(_soundActionKey));
            }
        }

        private void OnAnimationComplete(AnimKey animKey)
        {
            if (!IsRunned || _currentAnimKey != animKey)
            {
                return;
            }

            if (_repeatCount > 0)
            {
                SetAnimation();
                return;
            }
            
            if (_resourceCount <= 0)
            {
                Interrupt();
                return;
            }
            
            var addItemProtocol = new AddItemsProtocol(_itemId, _resourceCount, _unitGuid);
            _instantiator.Instantiate<AddItemsCommand>().Execute(addItemProtocol);
            Completed();
        }
        
        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
            }

            _animator = null;
            _audioModel = null;
            _soundActionKey = null;
            _unitGuid = null;
        }
    }
}