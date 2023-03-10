using System;
using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine.Profiling;

namespace BugsFarm.AstarGraph
{
    public class PointGraph : IPointGraph
    {
        public event Action OnUpdate;
        private readonly AstarPath _astar;
        private readonly PathModelStorage _pathModelStorage;
        private const string _defauitModelId = "0";
        private PointGroupe[] _sceneGroupes;
        private PathLimitationArea[] _sceneLimitations;
        private readonly List<string> _opened;
        
        public PointGraph(AstarPath astar, PathModelStorage pathModelStorage)
        {
            _astar = astar;
            _pathModelStorage = pathModelStorage;
            _opened = new List<string>();
        }
        public void Initialize()
        {
            var scenePathModel = _pathModelStorage.Get(_defauitModelId);
            _sceneGroupes = scenePathModel.PointGroupe;
            _sceneLimitations = scenePathModel.LimitationAreas;
            Reset();
        }
        public void OpenGroupe(string id)
        {
            // Предотвращение повторных действий
            if (_opened.Contains(id))
            {
                return;
            }

            var graphLock = _astar.PausePathfinding();
            var filter = NodeFilter.Empty()
                        .HasDependency(id)
                        .OnlyGroupe(id)
                        .Any();
            var processor = new NodeProcessor(filter);
            _opened.Add(id);
            processor.OpenNode();
            ProcessNodes(processor);
            graphLock.Release();
            filter?.Return();
            OnUpdate?.Invoke();
        }
        public void Reset()
        {
            _opened.Clear();
            var graphLock = _astar.PausePathfinding();
            BatchGroupe(_sceneGroupes, out var batchedNodes);
            AddNodes(batchedNodes, false, true);
            
            var actions = new NodeProcessor();
            actions.UseLimitation(_sceneLimitations);
            ProcessNodes(actions);
            graphLock.Release();
        }

        private void BatchGroupe(IEnumerable<PointGroupe> groupes, out List<Node> nodes)
        {
            nodes = new List<Node>();
            var nestedGroups = groupes.SelectMany(x => x.GetGroups());
            foreach (var groupe in nestedGroups)
            {
                if(groupe.IsParent) continue;
                nodes.AddRange(groupe.GetPoints());
            }
        }
        private void AddNodes(List<Node> nodes, bool walkable = false, bool destroyPrevious = false)
        {
            var graph = _astar.data.pointGraph;
            if(destroyPrevious && graph is IGraphInternals graphInternal)
            {
                graphInternal.DestroyAllNodes();
            }
                
            if (graph != null && nodes.Count > 0)
            {
                var nodeConnectors = nodes.OfType<NodeConnector>();
                var nodeJoints = nodes.OfType<NodeJoint>();
                    
                foreach (var connector in nodeConnectors)
                {
                    foreach (var joint in nodeJoints)
                    {
                        if (joint.Contains(connector.Position))
                        {
                            connector.AddConnection(joint, 1);
                            joint.AddConnection(connector, 1);
                        }

                        if (!connector.Excluded && joint is NodeJointExclusion nodeJointExclusion && nodeJointExclusion.HasExclusion(connector.Position))
                        {
                            NeighboursExclusion(connector, nodeJointExclusion);
                        }
                    }
                }
                graph.AddNode(nodes.ToArray(), walkable);
            }
        }

        private void NeighboursExclusion(Node target, NodeJointExclusion jointExclusion)
        {
            if(target == null || jointExclusion == null) return;

            target.Excluded = true;
            var connections = target.connections;
            if(connections == null || connections.Length == 0) return;

            foreach (var connection in connections)
            {
                if (connection.node is Node nodeConnection && !nodeConnection.Excluded && jointExclusion.HasExclusion(nodeConnection.Position))
                {
                    NeighboursExclusion(nodeConnection, jointExclusion);
                }
            }
        }
        private void ProcessNodes(NodeProcessor processor, Action callback = null)
        {
            var graph = _astar.data.pointGraph;
            var nodes = graph.nodes.OfType<Node>();
                
            foreach (var node in nodes)
            {
                processor.Process(node);                    
            }
            callback?.Invoke();
        }
    }
}

