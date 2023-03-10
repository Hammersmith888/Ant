using System;
using System.Collections.Generic;
using BugsFarm.Services.UIService;
using Malee.List;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [CreateAssetMenu(fileName = "CameraVerticalMovementModel", menuName = "Config/Camera/VerticalMovementModel")]
    public class CameraVerticalMovementModel : ScriptableObject
    {
        public IEnumerable<GameObject> Targets => _targets;
        public DOSettings AnimationSetup => _animationSetup;
        
        [Serializable]
        private class RTargets : ReorderableArray<GameObject>{}

        [SerializeField] private DOSettings _animationSetup;
        [Reorderable] [SerializeField] private RTargets _targets = new RTargets();
    }
}