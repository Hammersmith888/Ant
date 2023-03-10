using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.UnitSystem
{
    /// <summary>
    /// Не храните экземпляр этого класса,
    /// после обработки запроса экземпляр уходит в пул.
    /// </summary>
    public class PathHelperQuery
    {
        private static readonly List<PathHelperQuery> _freeInstances = new List<PathHelperQuery>();
        public Vector2[] SourcePoints { get; private set; }
        public int TraversableTags { get; private set; }
        public string ModelID { get; private set; }
        public string GraphMaskModelID { get; private set; }
        public bool Project { get; private set; }
        private bool _returned;
        private PathHelperQuery(params Vector2[] sourcePoints)
        {
            Default(sourcePoints);
        }

        private void Default(params Vector2[] sourcePoints)
        {
            SourcePoints = sourcePoints ?? new Vector2[0];
            ModelID = "-1";
            GraphMaskModelID = "-1";
            TraversableTags = -1;
            Project = false;
            _returned = false;
        }

        public static PathHelperQuery Empty(params Vector2[] sourcePoints)
        {
            while (_freeInstances.Count > 0)
            {
                var instance = _freeInstances[0];
                _freeInstances.RemoveAt(0);
                if (instance != null)
                {
                    instance.Default(sourcePoints);
                    return instance;
                }
            }
            
            return new PathHelperQuery(sourcePoints);
        }
        
        /// <summary>
        /// Вернуть экземпляр, чтобы не удалять его из памяти, локальный пул.
        /// Этот запрос используется очень часто.
        /// </summary>
        public void Return()
        {
            if (_returned)
            {
                return;
            }
            _returned = true;
            _freeInstances.Add(this);
        }

        public PathHelperQuery UseLimitationsCheck(string unitModelId)
        {
            ModelID = unitModelId;
            return this;
        }

        public PathHelperQuery UseTraversableCheck(int bitwiseMask)
        {
            TraversableTags = bitwiseMask;
            return this;
        }

        public PathHelperQuery ProjectPosition()
        {
            Project = true;
            return this;
        }

        public PathHelperQuery UseGraphMask(string graphMaskModelId)
        {
            GraphMaskModelID = graphMaskModelId;
            return this;
        }
    }
}