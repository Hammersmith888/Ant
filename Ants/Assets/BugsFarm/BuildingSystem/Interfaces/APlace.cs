using System.Collections.Generic;
using System.Linq;
using BugsFarm.Graphic;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public abstract class APlace : MonoBehaviour
    {
        public IEnumerable<Vector2> BoundPoints => _boundCollider.points.Select(x => (Vector2) transform.localToWorldMatrix.MultiplyPoint3x4(x + _boundCollider.offset));
        public Collider2D BoundCollider => _boundCollider;
        public Vector2 TocuhSizeMultiplier => _tocuhSizeMultiplier;
        public LocationLayer Layer => new LocationLayer(_viewRenderer.sortingLayerID, _viewRenderer.sortingOrder);
        public string ModelID => _modelID.ToString();

        [SerializeField] protected int _modelID;
        [SerializeField] protected Renderer _viewRenderer;
        [SerializeField] private PolygonCollider2D _boundCollider;
        [SerializeField] private Vector2 _tocuhSizeMultiplier = Vector2.one;

        public void Activate(bool visible)
        {
            OnChangeVisible(visible);
        }
        
        protected virtual void OnChangeVisible(bool visible)
        {
            gameObject.SetActive(visible);
        }

        protected virtual void OnValidate()
        {
            if (!_viewRenderer)
            {
                _viewRenderer = GetComponentInChildren<Renderer>();
            }

            if (!_boundCollider)
            {
                _boundCollider = GetComponentInChildren<PolygonCollider2D>();
            }

            if (_boundCollider)
            {
                _boundCollider.enabled = true;
                _boundCollider.isTrigger = true;
            }
        }

    #if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            var fillColor = Color.green;
            fillColor.a = 0.3f;

            var maxBounds = _boundCollider.bounds;
            maxBounds.Expand(_tocuhSizeMultiplier - Vector2.one);
            
            var rect = new Rect(maxBounds.min, maxBounds.size);
            Handles.DrawSolidRectangleWithOutline(rect, fillColor, Color.green);
        }
        #endif
    }
}