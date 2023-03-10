using BugsFarm.Graphic;
using Pathfinding;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class PositionInfo : INode
    {
    #region Properties

        public string Group { get; set; }
        
        public Vector2 Normal { get; set; }

        public Vector2 Position { get; set; }

        public uint Tag { get; set; }

        public uint Penalty { get; set; }

        public bool Walkable
        {
            get => _fromNode?.Walkable ?? _walkable;
            set => _walkable = _fromNode?.Walkable ?? value;
        }

        public bool Excluded
        {
            get => _fromNode?.Excluded ?? _excluded;
            set => _excluded = _fromNode?.Excluded ?? value;
        }

        public string LayerName { get; set; }
        public int LayerIndex { get; set; }

    #endregion

    #region Fields

        private bool _walkable;
        private bool _excluded;
        private INode _fromNode;
        
    #endregion

        public PositionInfo(NodeData data)
        {
            Init(data);
        }

        private void Init(NodeData data)
        {
            _fromNode = data.FromNode;
            Group = data.Group;
            Normal = data.Normal;
            LayerName = data.Layer;
            LayerIndex = SortingLayers.NameToLayerIndex(LayerName);
            Position = data.Position;
            Tag = data.Tag;
            Penalty = data.Penalty;
            _walkable = data.Walkable;
            _excluded = data.Excluded;
        }

        public static implicit operator PositionInfo(GraphNode other)
        {
            if (other is INode node)
            {
                return new PositionInfo(new NodeData(node));
            }
            
            return new PositionInfo(new NodeData());
        }
    }
}