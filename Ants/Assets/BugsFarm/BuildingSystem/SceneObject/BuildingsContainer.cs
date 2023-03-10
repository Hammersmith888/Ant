using UnityEngine;

namespace BugsFarm.BuildingSystem
{
    public class BuildingsContainer : MonoBehaviour
    {
        [SerializeField] private Transform _outsidePoint;
        public Transform Transform => transform;
        public Transform Outside => _outsidePoint;
    }
}