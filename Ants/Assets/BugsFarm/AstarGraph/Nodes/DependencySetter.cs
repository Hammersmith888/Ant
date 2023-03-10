using UnityEngine;

namespace BugsFarm.AstarGraph
{
    public class DependencySetter : MonoBehaviour
    {
        private enum DependencyType {Walkable, NotWalkable}
        public bool DependencyWalkable => _dependency == DependencyType.Walkable;
        public int PathID => _pathId;

        [SerializeField] private DependencyType _dependency = DependencyType.Walkable;
        [SerializeField] private int _pathId = -1;
    }
}