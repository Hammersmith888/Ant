using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AstarGraph;
using BugsFarm.ReloadSystem;
using UniRx;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace BugsFarm.UnitSystem
{
    public class PathHelper : IInitializable, IDisposable
    {
        private readonly GraphMaskModelStorage _graphMaskModelStorage;
        private readonly IPointGraph _pointGraph;
        private readonly AstarPath _astarPath;
        private readonly CellsArea _cellsArea;
        
        /// <summary>
        /// arg1 = Tags, arg2 = Layers, arg3 = Nodes
        /// </summary>
        private readonly Dictionary<uint, Dictionary<int, List<Node>>> _graphLayers;
        private List<Node>[,] _cellAreaNodes;
        private bool _finalized;
        private IDisposable _gameReloadingEvent;

        public PathHelper(GraphMaskModelStorage graphMaskModelStorage,
                          IPointGraph pointGraph,
                          AstarPath astarPath,
                          CellsArea cellsArea)
        {
            _graphMaskModelStorage = graphMaskModelStorage;
            _pointGraph = pointGraph;
            _astarPath = astarPath;
            _cellsArea = cellsArea;
            _graphLayers = new Dictionary<uint, Dictionary<int, List<Node>>>();
        }
        
        public void Initialize()
        {
            _pointGraph.OnUpdate += UpdateNodeDataBase;
            _gameReloadingEvent = MessageBroker.Default.Receive<GameReloadingReport>().Subscribe(OnGameReloading);
        }
        
        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            _pointGraph.OnUpdate -= UpdateNodeDataBase;
        }

        private void OnGameReloading(GameReloadingReport report)
        {
            _gameReloadingEvent.Dispose();
            Dispose();
        }
        public IEnumerable<INode> GetRandomNodes(PathHelperQuery query, int count = 1)
        {
            if (_finalized)
            {
                Debug.LogError($"{this} : System is finatized");
                query.Return();
                yield break;
            }
            
            if (count <= 0)
            {
                Debug.LogError($"{this} : Count need greater than zero");
                query.Return();
                yield break;
            }
            
            if (_graphLayers.Count == 0)
            {
                Debug.LogError($"{this} : Graph layers does not initialized");
                query.Return();
                yield break;
            }
            
            if (!_graphMaskModelStorage.HasEntity(query.GraphMaskModelID))
            {
                Debug.LogError($"{this} : {nameof(GraphMaskModel)} with id : '{query.GraphMaskModelID}' does not exist");
                query.Return();
                yield break;
            }
            
            var graphMaskModel = _graphMaskModelStorage.Get(query.GraphMaskModelID);
            var selectableTagMask    = new List<uint>();
            var selectableLayerMasks = new List<List<int>>();
            foreach (var graphTagLayer in _graphLayers)
            {
                if (_finalized)
                {
                    query.Return();
                    yield break;
                }
                
                if (!query.TraversableTags.IncludeBitwiseMask((int)graphTagLayer.Key))
                {
                    continue;
                }
                
                if (graphMaskModel.BitMaskTags.IncludeBitwiseMask((int)graphTagLayer.Key))
                {
                    var bitMaskLayers = graphMaskModel.Masks[graphTagLayer.Key].BitMaskLayers;
                    if (bitMaskLayers == 0)
                    {
                        continue;
                    } 
                    
                    var selectable = graphTagLayer.Value.Keys
                                     .Where(index => !bitMaskLayers.IncludeBitwiseMask(index))
                                     .ToList();
                    if (selectable.Count == 0)
                    {
                        continue;
                    }
                    
                    selectableTagMask.Add(graphTagLayer.Key);
                    selectableLayerMasks.Add(selectable);
                }
                else
                {
                    selectableTagMask.Add(graphTagLayer.Key);
                    selectableLayerMasks.Add(new List<int>(graphTagLayer.Value.Keys));
                }
            }

            if (selectableTagMask.Count == 0)
            {
                query.Return();
                yield break;
            }

            var cheked = new HashSet<INode>();
            var nodeFilter = NodeFilter.Empty()
                                       .OnlyWalkable()
                                       .ExcludeJoints()
                                       .ExcludeUnitLimitation(query.ModelID)
                                       .ExcludeConnectionsWith();
            while (count > 0)
            {
                if (_finalized)
                {
                    nodeFilter.Return();
                    query.Return();
                    yield break;
                }
                // случайный выбор тегов
                var randomIndex = Random.Range(0, selectableTagMask.Count);
                var randomTag = selectableTagMask[randomIndex];
                
                // случайный выбор слоя
                var tagLayers   = selectableLayerMasks[randomIndex];
                var randomLayer = tagLayers[Random.Range(0, tagLayers.Count)];
                
                // случайный выбор ноды
                var nodes = _graphLayers[randomTag][randomLayer];
                var rndNode = nodes[Random.Range(0, nodes.Count)];
                
                // исключение повтороной проверки
                if (rndNode == null || cheked.Contains(rndNode))
                {
                    continue;
                }
                
                cheked.Add(rndNode);
                if (_finalized)
                {
                    nodeFilter.Return();
                    query.Return();
                    yield break;
                }

                if (rndNode.connections == null)
                {
                    yield break;
                }
                
                foreach (var connection in rndNode.connections)
                {
                    if (_finalized)
                    {
                        nodeFilter.Return();
                        query.Return();
                        yield break;
                    }
                    var neighbourNode = (Node)connection.node;
                    if (neighbourNode != rndNode && nodeFilter.Match(neighbourNode))
                    {
                        count--;
                        query.Return();
                        yield return RandomLinearNode(rndNode, neighbourNode);
                    }
                    cheked.Add(neighbourNode);
                }
            }
            nodeFilter.Return();
            query.Return();
        }
        
        public bool HasNearestNodeAtPoints(PathHelperQuery query)
        {
            if (_finalized || !_astarPath)
            {
                Debug.LogError($"{this} : Something went wrong!");
                query.Return();
                return false;
            }

            if (query.SourcePoints.Length == 0)
            {
                query.Return();
                return true;
            }
            var filterNode = NodeFilter.Empty()
                                       .OnlyWalkable()
                                       .IncludeOnBitwiseMask(query.TraversableTags)
                                       .ExcludeUnitLimitation(query.ModelID)
                                       .ExcludeConnectionsWith();
            var maxDistSqr = _astarPath.maxNearestNodeDistanceSqr;
            foreach (var position in query.SourcePoints)
            {
                var cellIndices = _cellsArea.GetCellIndices(position, maxDistSqr);
                if (cellIndices == null)
                {
                    continue;
                }

                foreach (var cellIndex in cellIndices)
                {
                    var nodes = _cellAreaNodes[cellIndex.x, cellIndex.y];
                    if(nodes == null) continue;
                    var bestNode = default(Node);
                    var bestDist = float.PositiveInfinity;
                    foreach (var node in nodes)
                    {
                        if (!filterNode.Match(node))
                        {
                            continue;
                        }

                        var dist = (position - node.Position).sqrMagnitude;
                        if (dist < bestDist && dist < maxDistSqr) 
                        {
                            bestDist = dist;
                            bestNode = node;
                        }

                        if (bestNode != null)
                        {
                            filterNode.Return();
                            query.Return();
                            return true;
                        }
                    }
                }
            }
            filterNode.Return();
            query.Return();
            return false;
        }
        
        public INode FallRayCast(PathHelperQuery query)
        {
            if (_finalized)
            {
                Debug.LogError($"{this} : System is finatized");
                query.Return();
                return default;
            }
            
            if (!_astarPath)
            {
                Debug.LogError($"{this} : System is not initialized");
                query.Return();
                return default;
            }
            
            if (query.SourcePoints.Length == 0)
            {
                Debug.LogError($"{this} : Query points is empty");
                query.Return();
                return default;
            }
            
            var origin = query.SourcePoints[0];
            var initIndex = _cellsArea.GetCellIndicesExpandedNearestHorizontal(origin)?.ToArray();
            if (initIndex == null || initIndex.Length == 0)
            {
                query.Return();
                throw new IndexOutOfRangeException("You are trying to find nodes outside of cells!");
            }

            var nodeFilter = NodeFilter.Empty()
                                       .OnlyWalkable()
                                       .IncludeByNormalAngle(Vector2.up, maxAngle: 65)
                                       .ExcludeUnitLimitation(query.ModelID)
                                       .IncludeOnBitwiseMask(query.TraversableTags)
                                       .ExcludeConnectionsWith();
            var bestCost = float.MaxValue;
            var bestDistX = float.MaxValue;
            var bestNode = default(Node);
            // To exclude the wrong choice: more distant points and horizontal position.
            const float minPosX = 0.05f;
            const float maxDistanceError = 0.2f;
            var currY = initIndex[0].y;
            var maxY = _cellsArea.CellCount.y;
            
            while (currY < maxY)
            {
                foreach (var cellIndex in initIndex)
                {
                    var nodes = _cellAreaNodes[cellIndex.x, currY];
                    if (nodes == null || nodes.Count == 0)
                    {
                        continue;
                    }

                    foreach (var node in nodes)
                    {
                        if (!nodeFilter.Match(node) || node.Position.y >= origin.y)
                        {
                            continue;
                        }
                        var distX = Mathf.Abs(origin.x - node.Position.x);
                        var cost = Mathf.Abs(origin.y - node.Position.y) * Mathf.Max(distX, minPosX);
                        if (cost < bestCost)
                        {
                            bestNode = node;
                            bestCost = cost;
                            bestDistX = distX;
                        }
                    }
                }

                if (bestDistX <= maxDistanceError && bestNode != null)
                {
                    break;
                }
                currY++;
            }

            if (bestNode != null)
            {
                var result =  query.Project ? GetProjectedNode(query.ModelID, bestNode, origin) : bestNode;
                nodeFilter.Return();
                query.Return();
                return result;
            }
            nodeFilter.Return();
            query.Return();
            return default;
        }
        
        public INode GetNearestNodeAtPoint(PathHelperQuery query)
        {
            if (_finalized)
            {
                Debug.LogError($"{this} : System is finatized");
                query.Return();
                return default;
            }
            
            if (!_astarPath)
            {
                Debug.LogError($"{this} : System is not initialized");
                query.Return();
                return default;
            }
            
            if (query.SourcePoints.Length == 0)
            {
                Debug.LogError($"{this} : Query points is empty");
                query.Return();
                return default;
            }

            var position = query.SourcePoints[0];
            var maxDistSqr = _astarPath.maxNearestNodeDistanceSqr;
            var cellIndices = _cellsArea.GetCellIndices(position, maxDistSqr);
            var bestNode = default(Node);
            var bestDist = float.PositiveInfinity;
            var filterNode = NodeFilter.Empty()
                .OnlyWalkable()
                .IncludeOnBitwiseMask(query.TraversableTags)
                .ExcludeUnitLimitation(query.ModelID)
                .ExcludeConnectionsWith();
            foreach (var cellIndex in cellIndices)
            {
                var nodes = _cellAreaNodes[cellIndex.x, cellIndex.y];
                if(nodes == null || nodes.Count == 0) continue;
                foreach (var node in nodes)
                {
                    if (!filterNode.Match(node))
                    {
                        continue;
                    }
                    var dist = (position - node.Position).sqrMagnitude;
                    if (dist < bestDist && dist < maxDistSqr) 
                    {
                        bestDist = dist;
                        bestNode = node;
                    }
                }
            }
            filterNode?.Return();
            var result = query.Project ? GetProjectedNode(query.ModelID, bestNode, position) : bestNode;
            query.Return();
            return result;
        }
        
        private INode GetProjectedNode(string modelId, Node node, Vector3 source)
        {
            if(_finalized || node == null) return node;
            
            
            if(node.connections == null || node.connections.Length == 0)
            {
                return node;
            }

            foreach (var connection in node.connections)
            {
                if(_finalized) return default;
                var nighbour = (Node)connection.node;
                if (nighbour == null || nighbour.Tag != node.Tag || nighbour.HasUnitLimitation(modelId))
                {
                    continue;
                }

                Vector3 fromPoint = node.Position;
                Vector3 toPoint = nighbour.Position;
                
                var projectedPoint = Vector3.Project(source - fromPoint, toPoint - fromPoint) + fromPoint;
                
                // Если не вышли за границы линии, то мы нашли место по лучше.
                if (projectedPoint.Contains(fromPoint, toPoint))
                {
                    PositionInfo result = node;
                    var abDist = (fromPoint - toPoint).magnitude;
                    var normalT = (fromPoint - projectedPoint).magnitude / abDist;
                    result.Position = projectedPoint;
                    result.Normal = Vector3.Lerp(node.Normal, nighbour.Normal, normalT);
                    return result;
                }
            }
            return node;
        }
        
        private INode RandomLinearNode(INode node1, INode node2)
        {
            if (_finalized)
            {
                return node1 ?? node2;
            }
            var randomValue = Random.value;
            var data = new NodeData(randomValue <= 0.5f ? node1 : node2)
            {
                Position = Vector3.Lerp(node1.Position, node2.Position, randomValue),
                Normal = Vector3.Lerp(node1.Normal, node2.Normal, randomValue)
            };
            return new PositionInfo(data);
        }
        
        private void UpdateNodeDataBase()
        {
            if(_finalized) return;
            _graphLayers.Clear();
            _cellAreaNodes = new List<Node>[_cellsArea.CellCount.x, _cellsArea.CellCount.y];
            var filterMask = NodeFilter.Empty().WithoutExcluded();          
            var filterNode = NodeFilter.Empty().ExcludeJoints().OnlyWalkable().ExcludeConnectionsWith();
            foreach (var pointNode in _astarPath.data.pointGraph.nodes)
            {
                if (_finalized)
                {
                    filterMask?.Return();
                    filterNode?.Return();
                    return;
                }
                if (pointNode is Node node && filterNode.Match(node))
                {
                    if (!_graphLayers.ContainsKey(node.Tag))
                    {
                        _graphLayers.Add(node.Tag, new Dictionary<int, List<Node>>());
                    }

                    if (!_graphLayers[node.Tag].ContainsKey(node.LayerIndex))
                    {
                        _graphLayers[node.Tag].Add(node.LayerIndex, new List<Node>());
                    }
                    
                    var cellIndex = _cellsArea.GetCellIndex(node.Position);
                    if (cellIndex.HasValue)
                    {
                        var index0 = cellIndex.Value.x;
                        var index1 = cellIndex.Value.y;
                        var list = _cellAreaNodes[index0, index1];
                        if (list == null)
                        {
                            _cellAreaNodes[index0, index1] = list = new List<Node>();
                        }
                        list.Add(node);
                    }

                    if (filterMask.Match(node))
                    {
                        _graphLayers[node.Tag][node.LayerIndex].Add(node);
                    }
                }
            }
            filterMask?.Return();
            filterNode?.Return();
        }
    }
}