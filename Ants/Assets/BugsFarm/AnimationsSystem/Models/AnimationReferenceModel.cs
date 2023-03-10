using System;
using Spine.Unity;
using UnityEngine;
using UnityEngine.Serialization;

namespace BugsFarm.AnimationsSystem
{
    [Serializable]
    public struct AnimationReferenceModel
    {
        [SerializeField] private AnimationReferenceAsset _referenceAsset;
        [SerializeField] private float _timeScale;
        [SerializeField] private bool _isLoop;
        public AnimationReferenceAsset ReferenceAsset => _referenceAsset;
        public float TimeScale => _timeScale;
        public bool IsLoop => _isLoop;

        public AnimationReferenceModel(float timeScale, bool loop, AnimationReferenceAsset referenceAsset)
        {
            _timeScale = timeScale;
            _isLoop = loop;
            _referenceAsset = referenceAsset;
        }
    }
    
}