using System.Linq;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class NodeProcessor
    {
        private bool? _open;
        private PathLimitationArea[] _limitationAreas;
        private bool _hasLimitation;
        private readonly bool _injectedFilter;
        private NodeFilter _filter;
        public NodeProcessor()
        {
            _filter = NodeFilter.Empty();
        }
        public NodeProcessor(NodeFilter filter)
        {
            _injectedFilter = true;
            _filter = filter;
        }
        ~NodeProcessor()
        {
            foreach (var limitationArea in _limitationAreas)
            {
                limitationArea.CleanUp();
            }

            if (!_injectedFilter)
            {
                _filter?.Return();
            }
            _filter = null;
            _limitationAreas = null;
        }
        public void OpenNode(bool open = true)
        {
            _open = open;
        }
        public void UseLimitation(params PathLimitationArea[] areas)
        {
            if (areas.IsNullOrDefault())
            {
                Debug.LogError($"{nameof(UseLimitation)} :: {nameof(PathLimitationArea)} array is null");
            }
            _limitationAreas = areas;
            _hasLimitation = true;
        }
        public void Process(Node node)
        {
            if (!_filter.Match(node, out var hasDependency)) return;

            if (hasDependency && node is NodeDependency nodeDependency)
            {
                node.Walkable = nodeDependency.DependencyWalkable;
            }
            else if(_open.HasValue)
            {
                node.Walkable = _open.Value;
            }

            if (_hasLimitation)
            {
                foreach (var limitationArea in _limitationAreas)
                {
                    if (limitationArea.Contains(node.Tag, node.Position))
                    {
                        var limitations = limitationArea.GetUnitLimitations();
                        node.AddUnitLimitation(limitations.ToArray());
                    }
                }
            }
        }
    }
}