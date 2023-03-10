using System;
using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    /// <summary>
    /// Порядок вызова методов влияет на получение результата фильтра.
    /// PS : Проверку на HasDependency использовать в начале фильтра.
    /// </summary>
    public class NodeFilter
    {
        private static readonly List<NodeFilter> _freeInstances = new List<NodeFilter>();
        private readonly List<Func<Node,bool>> _checks;
        private bool _returned;
        private bool? _any;
        private bool _hasDependency;

        private NodeFilter()
        {
            _checks = new List<Func<Node, bool>>();
        }
        public static NodeFilter Empty()
        {
            while (_freeInstances.Count > 0)
            {
                var instance = _freeInstances[0];
                _freeInstances.RemoveAt(0);
                if (instance != null)
                {
                    instance._returned = false;
                    return instance;
                }
            }
            
            return new NodeFilter();
        }
        
        /// <summary>
        /// Вернуть экземпляр, чтобы не удалять его из памяти, локальный пул.
        /// Этот фильтр используется очень часто.
        /// </summary>
        public void Return()
        {
            if (_returned)
            {
                return;
            }
            
            _returned = true;
            _checks.Clear();
            _any = null;
            _hasDependency = false;
            _freeInstances.Add(this);
        }
        
        public NodeFilter OnlyGroupe(string groupe)
        {
            _checks.Add(node => node.Group == groupe);
            return this;
        }
        public NodeFilter IncludeOnBitwiseMask(int bitwiseMask)
        {
            _checks.Add(node => bitwiseMask == -1 || (bitwiseMask & (1<<(int)node.Tag)) != 0);
            return this;
        }
        public NodeFilter ExcludeOnBitwiseMask(int bitwiseMask)
        {
            _checks.Add(node => bitwiseMask == -1 || (bitwiseMask & (1<<(int)node.Tag)) == 0);
            return this;
        }
        public NodeFilter WithoutExcluded()
        {
            _checks.Add(node => !node.Excluded);
            return this;
        }
        public NodeFilter OnlyExcluded()
        {
            _checks.Add(node => node.Excluded);
            return this;
        }
        public NodeFilter ExcludeJoints()
        {
            _checks.Add(node => !node.IsJoint());
            return this;
        }
        public NodeFilter OnlyJoints()
        {
            _checks.Add(node => node.IsJoint());
            return this;
        }
        public NodeFilter HasDependency(string groupe)
        {
            _checks.Add(node =>
            {
                _hasDependency = false;
                if (string.IsNullOrEmpty(groupe) || !(node is NodeDependency nodeDependency)) return false;
                
                return _hasDependency = (groupe == nodeDependency.DependencyID.ToString());
            });
            return this;
        }
        public NodeFilter OnlyWalkable()
        {
            _checks.Add(node => node.Walkable);
            return this;
        }
        public NodeFilter OnlyUnWalkable()
        {
            _checks.Add(node => !node.Walkable);
            return this;
        }
        public NodeFilter ExcludeConnectionsWith(int lessThanCount = 1)
        {
            _checks.Add(node => node.connections.Length >= lessThanCount);
            return this;
        }
        public NodeFilter ExcludeUnitLimitation(string modelId)
        {
            _checks.Add(node => !node.HasUnitLimitation(modelId));
            return this;
        }

        /// <summary>
        /// Exclude node if normal angle greater than Unsigned Angle argument.
        /// </summary>
        public NodeFilter IncludeByNormalAngle(Vector2 normalizedNormal, float maxAngle)
        {
            _checks.Add(node => Vector2.Angle(node.Normal, normalizedNormal) <= maxAngle);
            return this;
        }
        /// <summary>
        /// Любое совпадение
        /// </summary>
        public NodeFilter Any()
        {
            _any = true;
            return this;
        }

        public bool Match(Node node, out bool hasDependency)
        {
            var result = Match(node);
            hasDependency = _hasDependency;
            return result;
        }

        public bool Match(Node node)
        {
            if (node == null)
            {
                return false;
            }

            if (_checks.Count == 0)
            {
                return true;
            }

           
            if (_any.HasValue)
            {
                // Linq.Any
                for (var i = 0; i < _checks.Count; i++)
                {
                    if (_checks[i].Invoke(node))
                    {
                        return true;
                    }
                }
                return false;
            }

            // Linq.All
            for (var i = 0; i < _checks.Count; i++)
            {
                if (!_checks[i].Invoke(node))
                {
                    return false;
                }
            }

            return true;
            // Generated the grabage
            //return _any.HasValue ? _checks.Any(x => x.Invoke(node)) : _checks.All(x => x.Invoke(node));
        }
    }
}