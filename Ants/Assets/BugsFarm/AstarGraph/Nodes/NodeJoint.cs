using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class NodeJoint : Node
    {
        private readonly float _connectionDistance;
        private readonly Vector2 _offset;

        public NodeJoint(float connectionDistance, Vector2 offset, NodeData data) : base(data)
        {
            _offset = offset;
            _connectionDistance = connectionDistance;
        }
        public bool Contains(Vector2 point)
        {
            return (point - (Position + _offset)).magnitude <= _connectionDistance;
        }
    }
}