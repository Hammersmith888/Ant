using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class LayerSetter : MonoBehaviour
    {
        [SortingLayerSelector] [SerializeField] private string _layer;
        public string Layer => _layer;

        public void SetLayer(string layerName)
        {
            _layer = layerName;
        }
    }
}
