using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace BugsFarm.RoomSystem
{
    [RequireComponent(typeof(Collider2D))]
    public class RoomOpeanbleSceneObject : RoomBaseSceneObject, IPointerClickHandler
    {
        private IInputController<SceneLayer> _inputController;
        private IInstantiator _instantiator;
        [SerializeField] private Collider2D _triggerCollider;
        [SerializeField] protected GameObject _graphicObject;

        [Inject]
        private void Inject(IInputController<SceneLayer> inputController, 
                            IInstantiator instantiator)
        {
            _inputController = inputController;
            _instantiator = instantiator;
        }
        public override void ChangeIntreaction(bool value)
        {
            if(!_triggerCollider) return;
            
            _triggerCollider.enabled = value;
        }
        public override void ChangeVisible(bool value)
        {
            if (!_graphicObject) return;

            _graphicObject.SetActive(value);
        }

        protected override void OnValidate()
        {
            base.OnValidate();
            if (!_triggerCollider)
            {
                _triggerCollider = GetComponent<Collider2D>();
            }

            if (!_graphicObject)
            {
                _graphicObject = SelfContainer.GetComponentInChildren<Renderer>()?.gameObject;
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
            if (colliderHit != _triggerCollider)
                return;
            
            if(_inputController.Locked) return;

            var protocol = new InteractionProtocol(Id, SceneObjectType.Rooms);
            var command = _instantiator.Instantiate<InteractionCommand>();
            command.Execute(protocol);
        }
    }
}