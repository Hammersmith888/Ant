using System.Collections.Generic;
using System.Linq;
using Pathfinding;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class PathLimitationArea : MonoBehaviour
    {
        [Tooltip("Утанавливает какие типы путей область захватывает")]
        [NodeTagsSelector][SerializeField] private string[] _tags;
        [Tooltip("Утанавливает на каких юнитов накладывается ограничение")]
        [SerializeField] private int[] _unitModelIds;
        [Tooltip("Комопненты описывающие область влияния")]
        [SerializeField] private List<BaseModifier> _modifiersArea;
        private IEnumerable<Vector2[]> _vertexGroups = null;
        public bool Contains(uint nodeTag, Vector3 nodePosition)
        {
            if (_vertexGroups == null)
            {
                _vertexGroups = _modifiersArea.Select(x => x.GetVertices().ToArray());
            }
            return ContainsTag(nodeTag) && _vertexGroups.Any(vertexGroup => Polygon.ContainsPoint(vertexGroup,nodePosition));
        }
        public IEnumerable<string> GetUnitLimitations()
        {
            return _unitModelIds.Select(x=> x.ToString());
        }
        public void CleanUp()
        {
            _vertexGroups = null;
        }
        private bool ContainsTag(uint pathTag)
        {
            return _tags.Contains(NodeUtils.GetTagName(pathTag));
        }
        
    #if UNITY_EDITOR
        [ExposeMethodInEditor]
        private void CreatePolyModifier()
        {
            CreateModifier<PolygoneModifier>();
        }
        [ExposeMethodInEditor]
        private void CreateBoxModifier()
        {
            CreateModifier<BoxModifier>();
        }
        [ExposeMethodInEditor]
        private void SetAllTags()
        {
            _tags = AstarPath.active.GetTagNames();
        }
        private void CreateModifier<T>() where T: BaseModifier
        {
            var obj = new GameObject(typeof(T).Name);
            obj.transform.SetParent(transform);
            obj.transform.localPosition = Vector3.zero;
            if (_modifiersArea.IsNullOrDefault())
            {
                _modifiersArea = new List<BaseModifier>();
            }
            _modifiersArea.Add(obj.AddComponent<T>());
        }
    #endif
    }
}