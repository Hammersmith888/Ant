using BugsFarm.AnimationsSystem;
using BugsFarm.AudioSystem;
using BugsFarm.InventorySystem;
using BugsFarm.Services.SceneEntity;
using BugsFarm.TaskSystem;
using BugsFarm.UnitSystem;
using Random = UnityEngine.Random;

namespace BugsFarm.BuildingSystem
{
    public class GetResourceFromSceneItemTask : BaseTask
    {
        private readonly ResourceArgs _args;
        private readonly GetResourceController _resourceController;
        private readonly InventoryStorage _inventoryStorage;
        private readonly AnimatorStorage _animatorStorage;
        private readonly SceneEntityStorage _sceneEntityStorage;
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
        
        public GetResourceFromSceneItemTask(GetResourceController getResourceController,
                                            InventoryStorage inventoryStorage,
                                            AnimatorStorage animatorStorage,
                                            SceneEntityStorage sceneEntityStorage,
                                            AudioModelStorage audioModelStorage,
                                            UnitSceneObjectStorage unitSceneObjectStorage,
                                            ISoundSystem soundSystem)
        {
            _resourceController = getResourceController;
            _args = getResourceController.Args;
            _inventoryStorage = inventoryStorage;
            _animatorStorage = animatorStorage;
            _soundSystem = soundSystem;
            _sceneEntityStorage = sceneEntityStorage;
            _audioModelStorage = audioModelStorage;
            _unitSceneObjectStorage = unitSceneObjectStorage;
        }
        
        public override void Execute(params object[] args)
        {
            if (IsRunned)return;
            
            base.Execute(args);
            var unitGuid   = (string) args[0];
            _resourceCount = args.Length == 1 ? 1 : (int) args[1];
            
            var entity     = _sceneEntityStorage.Get(unitGuid);
            _sceneObject   = _unitSceneObjectStorage.Get(unitGuid);
            _animator      = _animatorStorage.Get(unitGuid);
            _unitInventory = _inventoryStorage.Get(unitGuid);
            _repeatCount   = _args.RepeatCount;    
            
            var sounds = _args.ActionSoundKeys;
            if (sounds.Length > 0)
            {
                _audioModel    = _audioModelStorage.Get(entity.GetType().Name);
                _soundActionKey = sounds[Random.Range(0, sounds.Length)];
            }
            
            _animator.OnAnimationComplete += OnAnimationComplete;
            SetAnimation();
        }

        private void SetAnimation()
        {
            _repeatCount--;
            _currentAnimKey = _args.ActionAnimKeys[Random.Range(0, _args.ActionAnimKeys.Length)];
            if (!string.IsNullOrEmpty(_soundActionKey) && _audioModel != null && _audioModel.HasAudioClip(_soundActionKey))
            {
                _soundSystem.Play(_sceneObject.transform.position,_audioModel.GetAudioClip(_soundActionKey));
            }
            _animator.SetAnim(_currentAnimKey);
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

            var items = _resourceController.GetItems(ref _resourceCount);
            if (_resourceCount <= 0)
            {
                Interrupt();
                return;
            }
            
            _unitInventory.AddItems(items);
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
            _unitInventory = null;
        }
    }
}