using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StorageService;
using Spine.Unity;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace BugsFarm.ChestSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class ChestSceneObject : MonoBehaviour, IStorageItem, IPointerClickHandler
    {
        public SkeletonAnimation MainSkeleton => _mainSkeleton;
        
        [SerializeField] private GameObject _selfContainer;
        [SerializeField] private SkeletonAnimation _mainSkeleton;
       
        private string _guid;
        private IInstantiator _instantiator;
        private IInputController<SceneLayer> _inputController;
        
        public string Id
        {
            get => _guid;
            set => _guid = value;
        }
        
        [Inject]
        private void Inject(string guid, 
                            IInstantiator instantiator,
                            IInputController<SceneLayer> inputController)
        {
            _guid = guid;
            _instantiator = instantiator;
            _inputController = inputController;
        }
        
        public void ChangeVisible(bool value)
        {
            if (_selfContainer)
            {
                _selfContainer.SetActive(value);
            }
        }
        
        public void ChangeInteractable(bool interactable)
        {
            if (_selfContainer)
            {
                if(_selfContainer.TryGetComponent(out Collider2D interactionCollider))
                {
                    interactionCollider.enabled = interactable;
                }
            }
        }
        
        private void OnValidate()
        {
            if (!_selfContainer)
            {
                _selfContainer = gameObject;
            }
        }
        
        public void OnPointerClick(PointerEventData eventData)
        {
            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            
            if (colliderHit != GetComponent<Collider2D>())
                return;
            
            if (_inputController.Locked)
            {
                return;
            }

            var protocol = new InteractionProtocol(_guid, SceneObjectType.Chests);
            var command = _instantiator.Instantiate<InteractionCommand>();
            command.Execute(protocol);
        }
    }
}