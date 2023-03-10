using System;
using System.Collections.Generic;
using BugsFarm.Graphic;
using BugsFarm.Services.InputService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StorageService;
using UnityEngine;
using UnityEngine.EventSystems;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public abstract class BuildingSceneObject : MonoBehaviour, IStorageItem, IPointerClickHandler
    {
        public string Id { get; private set; }
        public LocationLayer Layer { get; protected set; }


        [SerializeField] protected SceneObjectType _sceneObjectType = SceneObjectType.Building;
        [SerializeField] protected Renderer _mainRenerer;
        [SerializeField] protected Collider2D _collider;

        protected IInstantiator Instantiator;
        protected IInputController<SceneLayer> InputController;

        [Inject]
        private void Inject(string guid,
                            IInstantiator instantiator,
                            IInputController<SceneLayer> inputController)
        {
            Id = guid;
            Instantiator = instantiator;
            InputController = inputController;
        }

        public void SetInterractable(bool interactable)
        {
            if (TryGetComponent(out Collider2D interactionCollider))
            {
                interactionCollider.enabled = interactable;
            }
        }

        public abstract void SetAlpha(float alpha01);
        public abstract void SetObject(params object[] args);

        public virtual void SetLayer(LocationLayer layer)
        {
            Layer = layer;
            if (!_mainRenerer)
                return;

            _mainRenerer.sortingLayerID = layer.ID;
            _mainRenerer.sortingOrder = layer.Order;
            var transf = transform;
            transf.position = new Vector3(transf.position.x, transf.position.y, SortingLayerOrderProvider.Layers[layer.Name] / -10.0f);
        }

        public virtual void SetPlace(APlace place)
        {
            if (place == null)
            {
                throw new NullReferenceException($"{this} SetPlace :: Place does not exist.");
            }
            
            transform.position = place.transform.position;
            transform.rotation = place.transform.rotation;
            transform.localScale = place.transform.localScale;
        }

        protected virtual void OnValidate()
        {
            if (!_mainRenerer)
            {
                _mainRenerer = GetComponentInChildren<Renderer>();
            }
            
            if (!_collider)
            {
                _collider = GetComponentInChildren<Collider2D>();
            }
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            
            Collider2D colliderHit = Physics2D.OverlapPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

            if (colliderHit != _collider)
                return;
            
            Debug.Log("Building");
            // check if we can handle interaction
            if (InputController.Locked)
            {
                return;
            }

            // run interaction command
            var command = Instantiator.Instantiate<InteractionCommand>();
            var protocol = new InteractionProtocol(Id, _sceneObjectType);
            command.Execute(protocol);
        }
    }

    public static class SortingLayerOrderProvider
    {
        public static readonly Dictionary<string, int> Layers = new Dictionary<string, int>()
        {
            {"BackUnderGround", 0},
            {"Background", 1},
            {"MiddleBackground", 2},
            {"MiddleGround", 3},
            {"MiddleObjectsGround", 4},
            {"Foreground", 5},
            {"ForeUnderGround", 6},
            {"ForeGroundObjects", 7},
            {"ForeAboveGround", 8},
            {"UIWorld", 9},
            {"NightMask", 10},
            {"UIScreen", 11},
        };
    }
}