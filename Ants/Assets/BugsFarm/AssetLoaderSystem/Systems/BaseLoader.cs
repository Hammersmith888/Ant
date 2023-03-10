using UnityEngine;

namespace BugsFarm.AssetLoaderSystem
{
    public abstract class BaseLoader<T> where T : Object
    {
        protected T Get(string path, bool catchError = false)
        {
            var loaded = Resources.Load<T>(path);
            if (catchError)
            {
                Assert(loaded, path);
            }
            return loaded;
        }

        protected void Assert(Object target, string path = "")
        {
            if (!target)
            {
                Debug.LogError($"{typeof(T).Name} does not found at path : {path}");
            }
        }
        protected bool Assert(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                Debug.LogError($"{typeof(T).Name} path is empty");
                return true;
            }

            return false;
        }
    }
}