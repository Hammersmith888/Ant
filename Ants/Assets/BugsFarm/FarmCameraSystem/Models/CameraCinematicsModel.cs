using System;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace BugsFarm.FarmCameraSystem
{
    [CreateAssetMenu(fileName = "CameraCinematicsModel", menuName = "Config/Camera/CinematicsModel")]
    public class CameraCinematicsModel : ScriptableObject
    {
        [Serializable]
        private class RCinematicPressets : SerializableDictionaryBase<string,GameObject>{}

        [SerializeField] private RCinematicPressets _pressets = new RCinematicPressets();

        public GameObject GetCinematic(string id)
        {
            return HasCinematic(id) ? _pressets[id] : default;
        }

        public bool HasCinematic(string id)
        {
            return _pressets.ContainsKey(id);
        }
    }
}