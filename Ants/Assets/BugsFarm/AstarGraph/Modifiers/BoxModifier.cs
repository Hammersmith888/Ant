using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class BoxModifier : BaseModifier
    {
        [Tooltip("Размер области модификатора")]
        [SerializeField] private Vector2 _size = Vector2.one;
        [Tooltip("Сдвиг области модификатора")]
        [SerializeField] private Vector2 _offset = Vector2.zero;
        
        public override IEnumerable<Vector2> GetVertices()
        {
            var bounds = new Bounds((Vector2)transform.position + _offset, (Vector2)transform.localScale + _size);
            var area = new Vector2[]
            {
                bounds.max,                    // topRight
                bounds.max.SetX(bounds.min.x), // topLeft
                bounds.min,                    // buttomleft
                bounds.min.SetX(bounds.max.x)  // buttomRight
            };
            return area;
        }
    }
}