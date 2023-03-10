using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public abstract class BaseModifier : MonoBehaviour
    {
        [Tooltip("Инвертировать результат модификации")]
        [SerializeField] private bool _invert = false;
        public bool Invert => _invert;
        public abstract IEnumerable<Vector2> GetVertices();
        
    #if UNITY_EDITOR
        private static readonly Color _edgeColor = Color.green;
        private static readonly Color _pointColor = Color.cyan;
        private static readonly Color _startPointColor = Color.yellow;
        protected virtual void OnDrawGizmos()
        {
            var area = GetVertices().ToArray();
            for (var i = 0; i < area.Length; i++)
            {
                var point = area[i];
                var pointNext = i + 1 < area.Length? area[i + 1] : area[0];
                Gizmos.color = _edgeColor;
                Gizmos.DrawLine(point, pointNext);
                
                Handles.Label(point, i.ToString());
                Gizmos.color = i == 0 ? _startPointColor : _pointColor;
                Gizmos.DrawSphere(point,0.1f);
            }
        }
    #endif
    }
}