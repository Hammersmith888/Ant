using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class NodeJointExclusion : NodeJoint
    {
        private readonly float _exclusionDistance;
        private readonly Vector2 _exclusionOffset;
        public NodeJointExclusion(float connectionDistance, 
                                  Vector2 offset,
                                  float exclusionDistance, 
                                  Vector2 exclusionOffset,
                                  NodeData data) : base(connectionDistance, offset, data)
        {
            _exclusionDistance = exclusionDistance;
            _exclusionOffset = exclusionOffset;
        }
        
        public bool HasExclusion(Vector2 point)
        {
            return (point - (Position + _exclusionOffset)).magnitude <= _exclusionDistance;
        }
    }
}