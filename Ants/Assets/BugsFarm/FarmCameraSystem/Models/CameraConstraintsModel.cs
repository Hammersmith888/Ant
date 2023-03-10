using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [CreateAssetMenu(fileName = "CameraConstraintsModel", menuName = "Config/Camera/ConstraintsModel")]
    public class CameraConstraintsModel : ScriptableObject
    {
        [Serializable]
        private class DictConstraintPressets : SerializableDictionaryBase<string, ConstraintsPresset>{}

        public GameObject CalibrationPresset => _сalibrationPresset;
        [SerializeField] private GameObject _сalibrationPresset;
        [SerializeField] private DictConstraintPressets _pressets;


        public ConstraintsPresset GetConstraints(string id)
        {
            return HasConstraint(id) ? _pressets[id] : default;
        }

        public bool HasConstraint(string id)
        {
            return _pressets.ContainsKey(id);
        }
    }
}