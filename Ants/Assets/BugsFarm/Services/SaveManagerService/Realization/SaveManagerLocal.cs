using UnityEngine;

namespace BugsFarm.Services.SaveManagerService
{
    public class SaveManagerLocal : ISaveManagerLocal
    {
        private readonly ISerializeHelper _serializeHelper;
        public SaveManagerLocal()
        {
            _serializeHelper = new SerializeHelperLocal();
        }
        
        public bool HasSaves(string saveDataKey)
        {
            return !string.IsNullOrEmpty(saveDataKey) && _serializeHelper.HasSerializedData(saveDataKey);
        }

        public void Save(string saveDataKey, string data)
        {
            if (string.IsNullOrEmpty(saveDataKey))
            {
                Debug.LogError($"{this} : The key must not be Null or Empty");
                return;
            }
            
            _serializeHelper.SaveSerializedData(data, saveDataKey);
        }

        public void Remove(string saveDataKey)
        {
            if (!HasSaves(saveDataKey))
            {
                return;
            }
            _serializeHelper.DeleteSerializedData(saveDataKey);
        }

        public string Load(string saveDataKey)
        {
            if (HasSaves(saveDataKey))
            {
                return _serializeHelper.GetSerializedData(saveDataKey);
            }
            Debug.LogError($"{this} : No save data exists for the key : {saveDataKey}");
            return default;
        }
    }
}