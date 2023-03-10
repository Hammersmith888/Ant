using System;
using System.Linq;
using BugsFarm.Logger;
using UnityEngine;

namespace BugsFarm.Services.SaveManagerService
{
    public class SaveManager : ISaveManager
    {
        private readonly ISerializeHelper _serializeHelper;
        private readonly ISavableStorage _savableStorage;
        public SaveManager(ISerializeHelper serializeHelper,
                           ISavableStorage savableStorage)
        {
            _serializeHelper = serializeHelper;
            _savableStorage = savableStorage;
        }
        
        public bool HasSaves(string saveDataKey)
        {
            return !string.IsNullOrEmpty(saveDataKey) && _serializeHelper.HasSerializedData(saveDataKey);
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

        public void Save(string data, string saveDataKey)
        {
            if (string.IsNullOrEmpty(saveDataKey))
            {
                Debug.LogError($"{this} : The key must not be Null or Empty");
                return;
            }
            if (string.IsNullOrEmpty(data))
            {
                Debug.LogError($"{this} : The data must not be Null or Empty");
                return;
            }
            _serializeHelper.SaveSerializedData(data, saveDataKey);
        }

        public void SaveAll(string saveDataKey)
        {
            if (string.IsNullOrEmpty(saveDataKey))
            {
                Debug.LogError($"{this} : The key must not be Null or Empty");
                return;
            }

            var allSavables = _savableStorage.GetAll();
            var allSaveData = allSavables.Select(x => new SaveData {Key = x.GetTypeKey(), Data = x.Save()});

            BugsLogger.AddToLog($"Saving process, {allSaveData.Count()} items \n");
            foreach (var saveData in allSaveData)
            {
                BugsLogger.AddToLog(saveData.Key);
            }
            
            var allSaveDataArray = allSaveData.ToArray();
            var data = JsonHelper.ToJson(allSaveDataArray);
            /*var data = JsonHelper.ToJson(_savableStorage
                                             .GetAll()
                                             .Select(x => new SaveData {Key = x.GetTypeKey(), Data = x.Save()})
                                             .ToArray());*/
            _serializeHelper.SaveSerializedData(data, saveDataKey);
        }

        public void Remove(string saveDataKey)
        {
            if(!HasSaves(saveDataKey)) return;
            _serializeHelper.DeleteSerializedData(saveDataKey);
        }

        public void LoadAll(string saveDataKey)
        {
            if (!HasSaves(saveDataKey))
            {
                Debug.LogError($"{this} : No save data exists for the key : {saveDataKey}");
                return;
            }

            var loadedData = _serializeHelper.GetSerializedData(saveDataKey);
            var saveDatas = JsonHelper.FromJson<SaveData>(loadedData).ToDictionary(x => x.Key);

            BugsLogger.AddToLog($"Loading process, {saveDatas.Count} items \n");
            
            foreach (var savable in _savableStorage.GetAll())
            {
                var typeKey = savable.GetTypeKey();
                BugsLogger.AddToLog($"{typeKey}\n");
                if (!saveDatas.ContainsKey(typeKey))
                {
                    Debug.LogError($"{this} : FarmData does not exist Savable : {typeKey}");
                    continue;
                }

                savable.Load(saveDatas[typeKey].Data);
            }
        }
    }

    [Serializable]
    public struct SaveData
    {
        public string Key;
        public string Data;
    }
}