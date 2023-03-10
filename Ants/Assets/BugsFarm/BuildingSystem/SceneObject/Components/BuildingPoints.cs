using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BuildingPoints : MonoBehaviour
    {
        [SerializeField] private MB_PosSide[] _points;
        public IEnumerable<IPosSide> Points => _points;
        private void Awake()
        {
            foreach (var point in _points)
            {
                point.SetParent(transform);
            }
        }
    }
}