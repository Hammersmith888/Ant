using System;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [Serializable]
    public class ConstraintsPresset
    {
        public GameObject[] Prefabs => _presset;
        public bool AutoFitLeftBoundaries => _autoFitLeftBoundaries;
        public bool AllowPan => _allowPan;
        public float AutoFitMin => _autoFitMin;
        
        [SerializeField] private bool _autoFitLeftBoundaries;
        [SerializeField] private bool _allowPan;
        [SerializeField] private float _autoFitMin;
        [SerializeField] private GameObject[] _presset;
    }
}