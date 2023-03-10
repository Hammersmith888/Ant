using System;
using BugsFarm.BuildingSystem;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace BugsFarm.UnitSystem
{
    public class TapableUnitSceneObject : UnitSceneObject, IPointerClickHandler
    {
        [SerializeField] protected Collider2D _interactionCollider;
        
        private IInstantiator _instantiator;
        private IInputController<SceneLayer> _inputController;

        [Inject]
        private void Inject(IInstantiator instantiator, 
                            IInputController<SceneLayer> inputController)
        {
            _instantiator = instantiator;
            _inputController = inputController;
        }

        public override void SetInteraction(bool active)
        {
            if(!_interactionCollider) return;
            _interactionCollider.enabled = active;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (colliderHit != _interactionCollider)
                return;
            
            // check if we can handle interaction
            if (_inputController.Locked)
            {
                return;
            }
            
            // run interaction command
            var command = _instantiator.Instantiate<InteractionCommand>();
            var protocol = new InteractionProtocol(Id, SceneObjectType.Unit);
            command.Execute(protocol);
        }
    }
}