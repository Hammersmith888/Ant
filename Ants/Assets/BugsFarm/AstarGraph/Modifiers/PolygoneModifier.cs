using System;
using System.Collections.Generic;
using System.Linq;
using Malee.List;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [Serializable]
    public class RPointList : ReorderableArray<Transform>{}
    public class PolygoneModifier : BaseModifier
    {
        [Tooltip("Список точек для создания области модификации")]
        [Reorderable] [SerializeField] private RPointList _points = new RPointList();
        private int _numberOfPoints;
        public override IEnumerable<Vector2> GetVertices()
        {
            return _points.Select(x => (Vector2)x.position);
        }
        
        [ExposeMethodInEditor]
        protected void AddPoint()
        {
            var newPoint = new GameObject($"Point_{_numberOfPoints++}");
            newPoint.transform.SetParent(transform);
            newPoint.transform.localPosition = Vector3.zero;
            _points.Add(newPoint.transform);
        }
        [ExposeMethodInEditor]
        protected void RemovePoints()
        {
            _numberOfPoints = 0;
            while (transform.childCount > 0)
            {
                var child = transform.GetChild(0);
                DestroyImmediate(child.gameObject);
            }
            _points.Clear();
        }
    }
}