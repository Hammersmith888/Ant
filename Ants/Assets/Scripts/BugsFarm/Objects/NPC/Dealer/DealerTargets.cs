using System;
using BugsFarm.BuildingSystem;
using UnityEngine;
using Random = UnityEngine.Random;

namespace BugsFarm.Objects.NPC.Dealer
{
    [Serializable]
    public class DealerTargets : MonoBehaviour
    {
        [SerializeField] private MB_PosSide[] _targets;
        public IPosSide GetRandomPosition()
        {
            return _targets[Random.Range(0,_targets.Length)];
        }
    }
}