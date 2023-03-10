using System.Collections.Generic;
using System.Linq;
using BugsFarm.Graphic;
using Pathfinding;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class Node : PointNode, INode
    {
        /// <summary>
        /// Нормаль поверхности
        /// </summary>
        public Vector2 Normal { get; set; }
        /// <summary>
        /// Номер группы
        /// </summary>
        public string Group { get; set; }
        /// <summary>
        /// На каком слое находится данная нода
        /// </summary>
        public string LayerName { get; set; }
        /// <summary>
        /// На каком слое находится данная нода
        /// </summary>
        public int LayerIndex { get; set; }
        /// <summary>
        /// Позиция в мировых координатах
        /// </summary>
        public Vector2 Position { get; }

        public bool Excluded { get; set; }
        
        private List<string> _unitLimitationlist;
        public Node(NodeData data) : base(data.Astar)
        {
            Normal = data.Normal;
            Group = data.Group;
            LayerName = data.Layer;
            LayerIndex = SortingLayers.NameToLayerIndex(LayerName);
            SetPosition((Int3)(Vector3)data.Position);
            Position = data.Position;

            Tag = data.Tag;
            Walkable = data.Walkable;
            Penalty = data.Penalty;
        }
        
        public override void AddConnection (GraphNode node, uint cost) 
        {
            if (node == null) throw new System.ArgumentNullException();

            if (connections != null) 
            {
                for (var i = 0; i < connections.Length; i++) 
                {
                    if (connections[i].node == node) 
                    {
                        connections[i].cost = cost;
                        return;
                    }
                }
            }

            var connLength = connections?.Length ?? 0;
            var newconns = new Connection[connLength+1];
            
            for (var i = 0; i < connLength; i++) 
            {
                newconns[i] = connections[i];
            }

            newconns[connLength] = new Connection(node, cost);
            connections = newconns;
        }
        public bool HasUnitLimitation(params string[] modelIds)
        {
            if (modelIds.Length == 0 || _unitLimitationlist.IsNullOrDefault() || _unitLimitationlist.Count == 0) return false;

            return _unitLimitationlist.Any(modelIds.Contains);
        }
        
        public void AddUnitLimitation(params string[] modelIds)
        {
            if (_unitLimitationlist.IsNullOrDefault())
            {
                _unitLimitationlist= new List<string>(modelIds);
                return;
            }
            var result = _unitLimitationlist.Except(modelIds).ToArray();
            if(result.Length == 0) return;
            _unitLimitationlist.AddRange(result);
        }
        
        public void RemoveUnitLimitation(params string[] modelIds)
        {
            if(_unitLimitationlist.IsNullOrDefault() || _unitLimitationlist.Count == 0) return;
            foreach (var modelID in modelIds)
            {
                _unitLimitationlist.Remove(modelID);
            }
        }
    }
}

