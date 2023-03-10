using System;
using System.Collections.Generic;
using System.Linq;
using BugsFarm.Services.SaveManagerService;
using UnityEngine;

namespace BugsFarm.Services.StorageService
{
    public class Storage<T> : ISavable, IStorage<T>  where T : IStorageItem
    {
        public event Action<string> OnStorageChanged;
        protected readonly Dictionary<string, T> Dict = new Dictionary<string, T>();

        public int Count => Dict.Count;

        public virtual void Add(T value)
        {
            if (value == null)
            {
                Debug.LogError("value is null");
                return;
            }

            if (value.Id == null)
            {
                Debug.LogError("value.id is null");
                return;
            }

            if (HasEntity(value.Id))
            {
                Debug.LogError($"value id: {value.Id} already exists for {value.GetType().Name}, ID : {value.Id}");
                return;
            }
            
            Dict.Add(value.Id, value);
            OnStorageChanged?.Invoke(value.Id);
        }

        public virtual void Remove(string key)
        {
            if (!HasEntity(key))
            {
                throw new ArgumentException($"Key not presented : {key} in {GetType().Name}");
            }
            Dict.Remove(key);
            OnStorageChanged?.Invoke(key);
        }
        
        public virtual void Remove(T value)
        {
            if (value == null)
            {
                throw new ArgumentException($"Value is missing in {value.GetType().Name}");
            }
            Remove(value.Id);
        }

        public bool HasEntity(string id)
        {
            return !string.IsNullOrEmpty(id) && Dict.ContainsKey(id);
        }

        public bool Any()
        {
            return Count > 0;
        }

        public IEnumerable<T> Get()
        {
            return Dict.Values;
        }

        public T Get(string key)
        {
            if (!HasEntity(key))
            {
                throw new ArgumentException($"Key not presented : {key}  in {GetType().Name}");
            }
            return Dict[key];
        }

        public bool TryGet(string key, out T value)
        {
            return Dict.TryGetValue(key, out value);
        }

        public virtual void Clear()
        {
            Dict?.Clear();
            OnStorageChanged?.Invoke("empty");
        }

       public virtual string GetTypeKey()
        {
            return typeof(T).Name;
        }

        public virtual string Save()
        {
            return JsonHelper.ToJson(Dict.Values.ToArray());
        }

        public virtual void Load(string jsonData)
        {
            if (string.IsNullOrEmpty(jsonData))
            {
                throw new InvalidOperationException($"Json data is null for : {typeof(T).Name}");
            }
            Dict.Clear();
            
            var loadedData = JsonHelper.FromJson<T>(jsonData);
            
            if (loadedData == null)
            {
                throw new InvalidOperationException($"Json data is not loaded for : {typeof(T).Name}");
            }
            
            foreach (var item in loadedData)
            {

                if (item == null)
                {
                    throw new InvalidOperationException($"Json data loaded not correctly : {typeof(T).Name}");
                }
                if (string.IsNullOrEmpty(item.Id))
                {
                    throw new InvalidOperationException($"Json data loaded not correctly for : {typeof(T).Name}, ID : '{item.Id}'");
                }
                
                Dict.Add(item.Id, item);
            }
        }
    }
}