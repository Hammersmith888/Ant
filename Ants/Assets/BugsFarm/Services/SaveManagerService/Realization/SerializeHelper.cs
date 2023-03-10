using UnityEngine;

namespace BugsFarm.Services.SaveManagerService
{
    public class SerializeHelper : ISerializeHelper
    {
        public bool HasSerializedData(string key)
        {
            return !string.IsNullOrEmpty(key) && PlayerPrefs.HasKey(key);
        }
        
        public string GetSerializedData(string key)
        {
            if (HasSerializedData(key))
            {
                return PlayerPrefs.GetString(key);
            }

            Debug.LogError($"[SerializeHelper::GetSerializedData] can`t load ['{key}']");
            return string.Empty;
        }
        
        public void SaveSerializedData(string data, string key)
        {
            if(string.IsNullOrEmpty(key) || string.IsNullOrEmpty(data))
            {
                Debug.LogError($"[SerializeHelper::SaveSerializedData] can`t save empty ['{key}', '{data}' ]");
                return;
            }

            PlayerPrefs.SetString(key, data);
            PlayerPrefs.Save();
        }

        public void DeleteSerializedData(string key)
        {
            if (!HasSerializedData(key))
            {
                Debug.LogError($"[SerializeHelper::SaveSerializedData] can`t remove ['{key}']");
                return;
            }
            PlayerPrefs.DeleteKey(key);
        }
    }
}