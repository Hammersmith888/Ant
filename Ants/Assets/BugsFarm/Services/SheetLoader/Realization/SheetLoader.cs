using System;
using System.Collections;
using System.Collections.Generic;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;
using UnityEngine.Networking;

namespace BugsFarm.Services.SheetLoader
{
    [ExecuteInEditMode]
    public abstract class SheetLoader : MonoBehaviour
    {
        [Serializable]
        private struct Sheet
        {
            public string Gid;
            public string TableID;
        }
        [Serializable] private class RSheetDict : SerializableDictionaryBase<string, Sheet> { }

        [Tooltip("Table sheet contains sheet id and tableId." +
                 "Table id on Google Spreadsheet. Let's say your table has the following url " +
                 "https://docs.google.com/spreadsheets/d/1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4/edit#gid=331980525So " +
                 "your table id will be '1RvKY3VE_y5FPhEECCa5dv4F7REJ7rBtGzQg9Z_B_DE4' and sheet id will be '331980525' (gid parameter)")]
        [SerializeField] private RSheetDict _sheets = new RSheetDict();

        private readonly List<string> _loading = new List<string> ();
        private const string _urlPattern = "https://docs.google.com/spreadsheets/d/{0}/export?format=csv&gid={1}";
        
        
        /// <summary>
        /// Async return string CSV data
        /// </summary>
        protected void Load(string sheetName, Action<string> onLoaded)
        {
            if (_loading.Contains(sheetName))
            {
                Debug.LogWarning($"{this} : Alredy Loading : {sheetName}");
                return;
            }
            
            if (!_sheets.ContainsKey(sheetName))
            {
                Debug.LogError($"{this} : Sheet id does not exist : {sheetName} ");
                return;
            }
            
            _loading.Add(sheetName);
            StartCoroutine(LoadCoroutine(sheetName, onLoaded));
        }

        private IEnumerator LoadCoroutine(string sheetName, Action<string> onLoaded)
        {
            var sheet = _sheets[sheetName];
            var url = string.Format(_urlPattern, sheet.TableID, sheet.Gid);
            Debug.Log($"Downloading: <color=yellow>{sheetName}</color> ...");

            var request = UnityWebRequest.Get(url);
            if (!request.isDone)
            {
                yield return request.SendWebRequest();
            }

            if (request.error == null)
            {

                Debug.Log($"Downloaded : <color=green>{sheetName}</color>");
                _loading.Remove(sheetName);
                onLoaded?.Invoke(request.downloadHandler.text);
                yield break; 
            }
                
            _loading.Remove(sheetName);
            Debug.LogError($"{this} : {request.error}");
        }
    }
}