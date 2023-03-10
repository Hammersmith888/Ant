using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class TasksPoints : MonoBehaviour
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

        [ExposeMethodInEditor]
        private void FindInChildren()
        {
            _points = GetComponentsInChildren<MB_PosSide>(true);
        }
    }
}