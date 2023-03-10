using UnityEditor;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    [ExecuteInEditMode]
    public class NodeJointSetter : MonoBehaviour
    {
        [SerializeField] private Vector2 _offset = Vector2.zero;
        [Header("Настройка исключений")]
        [SerializeField] private bool _useExclusion = false;
        [SerializeField] private Vector2 _exclusionOffset = Vector2.zero;
        [SerializeField] private float _exclusionRadius = 1f;

        public float ConnectionDistance => Mathf.Abs((transform.localScale.x + transform.localScale.y) / 2f);
        public bool Contains(Vector3 point) => (point - ModifiedPosition).magnitude <= ConnectionDistance;
        public Vector3 Position => transform.position;
        public Vector3 Offset => _offset;
        private Vector3 ModifiedPosition => Position + Offset;
        
        public bool NodeExclusion => _useExclusion;
        public Vector3 ExclusionOffset => _exclusionOffset;
        public float ExclusionDistance => Mathf.Abs(ConnectionDistance * _exclusionRadius);
        public bool HasExclusion(Vector3 point) => _useExclusion && (point - ModifiedExclusionPosition).magnitude <= ExclusionDistance;
        private Vector3 ModifiedExclusionPosition => ModifiedPosition + (Vector3)_exclusionOffset;
        
    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.yellow;
            Handles.Disc(Quaternion.identity, ModifiedPosition, Vector3.forward, ConnectionDistance, false, 0);
            
            if (_useExclusion)
            {
                Handles.color = Color.magenta;
                Handles.Disc(Quaternion.identity, ModifiedExclusionPosition, Vector3.forward, ExclusionDistance, false, 0);
            }

            Gizmos.color = Color.yellow;
            Gizmos.DrawSphere(Position,0.02f);
        }
    #endif
    }
}
