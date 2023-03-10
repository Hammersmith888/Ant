using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.AstarGraph;
using BugsFarm.BuildingSystem;
using Pathfinding;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UnitPath
    {
        public event Action<bool> OnPathCalculated;
        public bool IsPathComplete { get; private set; }
        public INode Next => GetNextNode();
        public INode End => _path.Count > 0 ? _path[_path.Count - 1] : null;

        public INode BeforeEnd
        {
            get
            {
                if (_path.Count <= 0)
                {
                    return null;
                }

                var targetIndex = _path.Count - 2; // Count -1 = EndPoint, Count - 2 = BeforEnd;
                return targetIndex < 0 ? _path[0] : _path[targetIndex];
            }
        }

        public int TraversableTags => _seeker.traversableTags;

        private readonly ITraversalProvider _traversalProvider;
        private readonly Transform _agent;
        private readonly Seeker _seeker;
        private readonly List<INode> _path;
        private int _pathIndex;
        private bool _finalized;
        private Vector3 _endPoint;
        public UnitPath(Transform agent, ITraversalProvider traversalProvider)
        {
            _agent = agent;
            _traversalProvider = traversalProvider;
            _seeker = agent.GetComponent<Seeker>();
            _seeker.pathCallback += OnCalculatedPath;
            _path = new List<INode>();
        }
        
        public void StartPath(Vector3 endPosition)
        {
            if(_finalized) return;
            if (_seeker.IsNullOrDefault())
            {
                Debug.Log($"{this} : Seeker компонент отсутсвтует!!!");
                return;
            }

            IsPathComplete = false;
            _seeker.CancelCurrentPathRequest();
            _seeker.StartPath(CreatePath(endPosition));
        }

        public void ReachTarget()
        {
            IsPathComplete = true;
            _pathIndex = _path.Count - 1;
        }

        public void OverridePath(params INode[] nodes)
        {
            if(_finalized) return;
            if (nodes.IsNullOrDefault() || nodes.Length == 0) return;
            _pathIndex = 0;
            _path.Clear();
            _path.AddRange(nodes);
        }

        public void Dispose()
        {
            if(_finalized) return;
            _finalized = true;
            OnPathCalculated = null;
        }

        private void ReplacePointOfPath(int index, Vector3 point)
        {
            if(_finalized) return;
            if (!_path.IsNullOrDefault() && _path.Count > 0 && index < _path.Count)
            {
                var oldNode = _path[index];
                var newNode = new PositionInfo(new NodeData(oldNode)) {Position = point};
                _path[index] = newNode;
            }
        }
        
        private Path CreatePath(Vector3 endPoint)
        {
            if(_finalized) return default;
            endPoint.z = _agent.position.z;
            _endPoint = endPoint;
            var path = ABPath.Construct(_agent.position, endPoint);
            path.traversalProvider = _traversalProvider;
            path.enabledTags = _seeker.traversableTags;
            path.nnConstraint.graphMask = _seeker.graphMask;
            path.tagPenalties = _seeker.tagPenalties;
            return path;
        }

        private void OnCalculatedPath(Path calcPath)
        {
            if(_finalized) return;
            if (calcPath != null && !calcPath.error)
            {
                _path.Clear();
                _path.AddRange(calcPath.path.OfType<INode>());
                if (_path.Count > 0)
                {
                    _pathIndex = 0;
                    ReplacePointOfPath(_path.Count - 1, calcPath.vectorPath[calcPath.vectorPath.Count - 1]);
                }
                OnPathCalculated?.Invoke(true);
                return;
            }

            if (calcPath == null)
            {
                Debug.LogError($"{this} : Path not generated.");
            }
            else
            {
                Debug.LogError($"{this}: calcPath.error :    {calcPath.error}");
                Debug.LogError($"{this}: calcPath.CompleteState :    {calcPath.CompleteState}");
                Debug.LogError($"{this}: calcPath.errorLog :    {calcPath.errorLog}");
                Debug.LogError($"{this}: Start point of path :    {(Vector3S) _agent.position}");
                Debug.LogError($"{this}: End point of path :    {(Vector3S) _endPoint}");
            }

            _path.Clear();
            _pathIndex = 0;
            OnPathCalculated?.Invoke(false);
        }

        private INode GetNextNode()
        {
            if(_finalized) return default;
            if (_path.IsNullOrDefault() || _path.Count <= 0)
            {
                return default;
            }

            IsPathComplete = _pathIndex + 1 >= _path.Count;

            return IsPathComplete ? End : _path[_pathIndex++];
        }
    }
}