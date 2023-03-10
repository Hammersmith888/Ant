using UnityEngine;

namespace BugsFarm.UnitSystem
{
    public class UnitsContainer : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        public Transform Transform => transform;
        public Vector2 SpawnPoint => _spawnPoint.position;
    }
}