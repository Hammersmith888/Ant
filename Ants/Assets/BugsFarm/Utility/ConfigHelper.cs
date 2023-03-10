using System;
using System.IO;
using BugsFarm.Services.SaveManagerService;
using UnityEngine;

namespace BugsFarm.Utility
{
    public static class ConfigHelper
    {
        private const string _pathConfigs = "/BugsFarm/Resources/";
        private const string _extension = ".json";
        
        public static T[] Load<T>(string name)
        {
            var textAsset = Resources.Load<TextAsset>(name);
            if (!textAsset)
            {
                throw new NullReferenceException($"Json not loaded : {name}");
            }
            return JsonHelper.FromJson<T>(textAsset.text);
        }
        public static void Save<T>(T[] data, string name, bool prettyPrint = false)
        {
            if (data == null || data.Length == 0)
            {
                Debug.LogError($"Data is missing or empty");
            }
            
            var path = Application.dataPath + _pathConfigs + name + _extension;
            var jsonData = JsonHelper.ToJson(data, prettyPrint);
            File.WriteAllText(path, jsonData);
        }
    }
}