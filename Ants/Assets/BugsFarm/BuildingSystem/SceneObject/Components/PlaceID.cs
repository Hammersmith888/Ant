using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;
using UnityEditor;
using UnityEngine;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    [ExecuteInEditMode]
    public class PlaceID : MonoBehaviour, IStorageItem
    {
        [SerializeField] private int _groupeID = -1; // TODO : Remove
        [SerializeField] private uint _placeNumber;
        [SerializeField] private BoxCollider2D _touchArea; 

        private Dictionary<string, APlace> _places;

        public string PlaceNumber
        {
            get => _placeNumber.ToString();
            set
            {
                _placeNumber = uint.Parse(value); InitPlaces();
            }
        }

        public int GroupID => _groupeID;

        // TODO : Remove
        private Action<string> _onTriggerAction;
        
        string IStorageItem.Id => PlaceNumber;

        [Inject]
        private void Inject(PlaceIdStorage placeIDStorage)
        {
            InitPlaces();

            placeIDStorage.Add(this);
            _touchArea.enabled = false;
        }

        public APlace GetPlace(string modelID)
        {
            return HasPlace(modelID) ? _places[modelID] : default;
        }
        
        public bool HasPlace(string modelID)
        {
            return _places.ContainsKey(modelID);
        }
        
        public void InitPlaces()
        {
            var places = GetComponentsInChildren<APlace>(true);
            _places = places.ToDictionary(x => x.ModelID);
        }
        
        public void Activate(string modelId, Action<string> onTrigger)
        {
            var place = GetPlace(modelId);
            place.Activate(true);
            _touchArea.enabled = true;
            _onTriggerAction = onTrigger;
            
            Vector2 center = transform.worldToLocalMatrix.MultiplyPoint3x4(place.BoundCollider.bounds.center);
            _touchArea.size = place.BoundCollider.bounds.size * place.TocuhSizeMultiplier;
            _touchArea.offset = center;
        }
        
        public void DeActivate()
        {
            _touchArea.enabled = false;
            foreach (var place in _places)
            {
                place.Value.Activate(false);
            }
            _onTriggerAction = null;
        }

        private void OnMouseDown()
        {
            _onTriggerAction?.Invoke(PlaceNumber);
        }


    #if UNITY_EDITOR
        [SerializeField] private GUIStyle _style;
        private int _isSpawned;
        
        private void OnDrawGizmos()
        {
            if(_isSpawned < 5)
            {
                var copyStyle = new GUIStyle(_style) {normal = {textColor = Color.green}};
                Handles.Label(transform.position, PlaceNumber, copyStyle);
                _isSpawned++;
            }
            else
            {
                if (GroupID < 0)
                {
                    var copyStyle = new GUIStyle(_style) {normal = {textColor = Color.red}};
                    Handles.Label(transform.position, PlaceNumber, copyStyle);
                }
                else
                {
                    Handles.Label(transform.position, PlaceNumber, _style); 
                }

            }
        }

        private void OnDrawGizmosSelected()
        {
            var fillColor = Color.yellow;
            fillColor.a = 0.3f;
            var maxBounds = new Bounds(transform.position,Vector3.zero);
            foreach (var place in GetComponentsInChildren<APlace>(true))
            {
                if (place.gameObject.activeSelf)
                {
                    maxBounds.Encapsulate(place.BoundCollider.bounds);
                }
                else
                {
                    foreach (var point in place.BoundPoints)
                    {
                        maxBounds.Encapsulate(point);
                    }
                }
            }
            var rect = new Rect(maxBounds.min, maxBounds.size);
            Handles.DrawSolidRectangleWithOutline(rect, fillColor, Color.green);
        }
    #endif
    }
}

