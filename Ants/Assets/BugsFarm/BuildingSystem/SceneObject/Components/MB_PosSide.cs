using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Profiling;

namespace BugsFarm.BuildingSystem
{
    public class MB_PosSide : MonoBehaviour, IPosSide
    {
        public bool LookLeft => CalcLookLeft();
        private Transform _parent;
        private Transform _self;

        private void Awake()
        {
            _self = transform;
        }

        public Vector2 Position
        {
            get =>  _self == null ? Vector3.zero : _self.position;
            set => _self.position = value;
        }
        [SerializeField] private bool _lookLeft;

        public void SetParent(Transform parent)
        {
            _parent = parent;
        }

        private bool CalcLookLeft()
        {
            var container = _parent ? _parent : _self;
            if (!container) return _lookLeft;
            return container.localScale.x < 0 ? !_lookLeft : _lookLeft;
        }
    #if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Handles.color = Color.green;
            Handles.DrawSolidDisc(Position, Vector3.forward, 0.1f);
        }
    #endif
    }

    public static class PosSideUtils
    {
        public static Vector2[] ToPositions(this IEnumerable<IPosSide> points)
        {
            return points?.Select(x => x.Position).ToArray() ?? new Vector2[0];
        }
        public static Vector2[] ToPositions(this IPosSide point)
        {
            return point == null ? new Vector2[0] : new[] {(Vector2) point?.Position};
        }
    }
}