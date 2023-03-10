using System;
using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public class AddResourceToSceneItemTask : BaseTask
    {
        private readonly ResourceArgs _args;
        private readonly AddResourceController _resourceController;
        private readonly Action<int> _onResourceAdd;
        private readonly InventoryStorage _inventoryStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly SceneEntityStorage _entityController;
        private readonly AudioModelStorage _audioModelStorage;
        private readonly UnitSceneObjectStorage _unitSceneObjectStorage;
        private readonly ISoundSystem _soundSystem;

        private ISpineAnimator _animator;
        private AudioModel _audioModel;
        private int _repeatCount;
        private int _resourceCount;
        private string _soundActionKey;
        private IInventory _unitInventory;
        private AnimKey _currentAnimKey;
        private UnitSceneObject _sceneObject;

        public AddResourceToSceneItemTask(AddResourceController resourceController,
                                          Action<int> onResourceAdd,
                                          InventoryStorage inventoryStorage,
                                          AnimatorStorage animatorStorage,
                                          SceneEntityStorage entityController,
                                          AudioModelStorage audioModelStorage,
                                          UnitSceneObjectStorage unitSceneObjectStorage,
                                          ISoundSystem soundSystem)
        {
            _resourceController = resourceController;
            _onResourceAdd = onResourceAdd;
            _args = resourceController.Args;
            _inventoryStorage = inventoryStorage;
            _animatorStorage = animatorStorage;
            _soundSystem = soundSystem;
            _entityController = entityController;
            _audioModelStorage = audioModelStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }

        public override void Execute(params object[] args)
        {
            if (IsRunned) return;

            base.Execute(args);
            var unitGuid = (string) args[0];
            _resourceCount = (int) args[1];

            var entity = _entityController.Get(unitGuid);
            _sceneObject = _unitSceneObjectStorage.Get(unitGuid);
            _unitInventory = _inventoryStorage.Get(unitGuid);
            _animator = _animatorStorage.Get(unitGuid);
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

            if (string.IsNullOrEmpty(_soundActionKey) && _audioModel != null &&
                _audioModel.HasAudioClip(_soundActionKey))
            {
                _soundSystem.Play(_sceneObject.transform.position, _audioModel.GetAudioClip(_soundActionKey));
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
            
            // другие задачи могли не иметь в налиичии нужное кол-во предметов,
            // по этому проверим наличие у юнита доступных предметов.
            if (_unitInventory.HasItem(_resourceController.ItemID))
            {
                var itemSlot = _unitInventory.GetItemSlot(_resourceController.ItemID);
                _resourceCount = itemSlot.Count < _resourceCount ? itemSlot.Count : _resourceCount;
                _resourceController.AddItem(ref _resourceCount);
                _unitInventory.Remove(_resourceController.ItemID, _resourceCount);
                _onResourceAdd?.Invoke(_resourceCount);
                
                Completed();
                return;
            }
            
            Interrupt();
        }

        protected override void OnDisposed()
        {
            if (IsExecuted)
            {
                _animator.OnAnimationComplete -= OnAnimationComplete;
            }

            _animator = null;
            _audioModel = null;
            _repeatCount = _resourceCount = 0;
            _soundActionKey = null;
            _unitInventory = null;
            _currentAnimKey = AnimKey.None;
        }
    }
}