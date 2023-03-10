using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.StorageService;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEditor;
using UnityEngine;

namespace BugsFarm.AudioSystem
{
    [CreateAssetMenu(fileName = "AudioModel", menuName = "Config/Audio/AudioModel")]
    public class AudioModel : ScriptableObject, IStorageItem
    {
        [Serializable]
        private class RDictAudioClips : SerializableDictionaryBase<string,AudioClip>{}      
        [Serializable]
        private class RDictAudioClipsPaths : SerializableDictionaryBase<string, string>{}

        public string ModelID => _modelID;
        string IStorageItem.Id => _modelID;

        [SerializeField] private string _modelID;
        
    #if UNITY_EDITOR
        [SerializeField] private List<AudioClip> _audioClipsConverterList = new List<AudioClip>();
    #endif
        
        [SerializeField] private RDictAudioClipsPaths _audioStorage = new RDictAudioClipsPaths();
        
        public string GetAudioClip(string key)
        {
            if (string.IsNullOrEmpty(key) || !HasAudioClip(key))
            {
                Debug.LogError($"{this} : Does not have audio key : '{(string.IsNullOrEmpty(key) ? "string.IsNullOrEmpty" : key)}'");
                return default;
            }
            return _audioStorage[key];
        }

        public bool HasAudioClip(string key)
        {
            return !string.IsNullOrEmpty(key) && _audioStorage.ContainsKey(key);
        }
        
        public IEnumerable<string> GetAudioClips()
        {
            return _audioStorage.Values;
        }

    #if UNITY_EDITOR
        [ContextMenu("SetupAudioModel")]
        private void FillAudioPaths()
        {
            _audioStorage.Clear();
            foreach (var audioClip in _audioClipsConverterList)
            {
                var path = AssetDatabase.GetAssetPath(audioClip);

                const string resourcesName = "Resources/";
                const string extensionName = ".mp3";
                var resourceStartIndex = path.IndexOf(resourcesName, StringComparison.Ordinal);
                var newPath = path.Remove(0, resourceStartIndex + resourcesName.Length);
                newPath = newPath.Remove(newPath.Length - extensionName.Length, extensionName.Length);

                var audioClipName = audioClip.name.Split(new[]{"_"},StringSplitOptions.RemoveEmptyEntries).Last();
                _audioStorage.Add(audioClipName, newPath);
            }
        }
    #endif
    }
}