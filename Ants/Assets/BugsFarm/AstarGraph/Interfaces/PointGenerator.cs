using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public abstract class PointGenerator : MonoBehaviour
    {
        [Tooltip("Является группой родителем?")]
        [SerializeField] protected bool _isParent = false;
        
        [Tooltip("Слой по умолчанию. Используется для стартовой настройки.")]
        [SerializeField] [SortingLayerSelector] protected string _defaultLayer;
        public abstract IEnumerable<Node> GeneratePointsGroupe(NodeData data);
        protected abstract IEnumerable<Node> Connection(IEnumerable<Node> nodes);
    }
}