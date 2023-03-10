using BugsFarm.BuildingSystem;
using BugsFarm.Graphic;
using BugsFarm.Services.InputService;
using BugsFarm.Services.MonoPoolService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.SimulationSystem;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;
using TickableManager = Zenject.TickableManager;

namespace BugsFarm.UnitSystem
{
    public class UnitRipSceneObject : MonoBehaviour, IMonoPoolable, IPointerClickHandler
    {
        public GameObject GameObject => gameObject;

        [SerializeField] private SpriteRenderer _mainRenderer;
        [SerializeField] private Collider2D _interactionCollider;
        private IInstantiator _instantiator;
        private IInputController<SceneLayer> _inputController;
        private ISimulationSystem _simulationSystem;
        private IMonoPool _monoPool;
        private const float _fadeOutTime = 1f;
        private float _fadeTimer;
        private string _guid;
        
        [Inject]
        private void Inject(IInstantiator instantiator, 
                            IInputController<SceneLayer> inputController,
                            IMonoPool monoPool)
        {
            _instantiator = instantiator;
            _inputController = inputController;
            _monoPool = monoPool;

        }

        public void SetGuid(string guid)
        {
            _guid = guid;
        }
        public void SetAngle(Vector2 normal)
        {
            if(transform)
            {
                transform.rotation = Quaternion.LookRotation(Vector3.forward, normal);
            }
        }
        public void SetPosition(Vector2 position)
        {
            transform.position = position;
        }
        public void SetAlpha(float alpha01)
        {
            if(!_mainRenderer) return;
            
            var color = _mainRenderer.color;
            color.a = alpha01;
            _mainRenderer.color = color;
        }
        public void SetLayer(LocationLayer layer)
        {
            if (!_mainRenderer) return;
            _mainRenderer.sortingLayerID = layer.ID;
            _mainRenderer.sortingOrder = layer.Order;
        }
        public void SetActive(bool active)
        {
            if(!gameObject) return;
            gameObject.SetActive(active);
        }
        public void SetInteraction(bool active)
        {
            if(!_interactionCollider) return;
            _interactionCollider.enabled = active;
        }
        public void OnDespawned()
        {
            _guid = string.Empty;
            _instantiator = null;
            _inputController = null;
            
            SetActive(false);
            SetInteraction(false);
            SetAngle(Vector2.up);
            SetAlpha(0);
            _fadeTimer = 0;
        }
        public void OnSpawned()
        {
            _fadeTimer = 0;
            SetActive(true);
        }

        public void FadeOut()
        {
            _mainRenderer.DOFade(1.0f,  _fadeOutTime);
        }
        private void OnDestroy()
        {
            _monoPool.Destroy(this);
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            // check if we can handle interaction
            if (_inputController.Locked)
            {
                return;
            }
            
            // run interaction command
            var command = _instantiator.Instantiate<InteractionCommand>();
            var protocol = new InteractionProtocol(_guid, SceneObjectType.AntRip);
            command.Execute(protocol);
        }
    }
}

