using UnityEngine;

namespace BugsFarm.AssetLoaderSystem
{
    public class IconLoader : BaseLoader<Sprite>
    {
        private const string _postfix = "_ico";
        private const string _default = "default";
        private const string _currency = "Currency_";
        private readonly string[] _paths = 
        {
            "Icons/Food/",
            "Icons/Buildings/",
            "Icons/Decor/",
            "Icons/Units/", 
            "Icons/Misc/", 
            "Icons/",
        };

        public Sprite Load(string name)
        {
            if (Assert(name))
            {
                return default;
            }
            
            foreach (var path in _paths)
            {
                var loadObj = Get(path + name + _postfix);
                if (loadObj)
                {
                    return loadObj;
                }
            }

            Assert(default, name);
            return default;
        }
        
        public Sprite LoadOrDefault(string name, int pathIndex)
        {
            if (Assert(name))
            {
                return default;
            }
            
            if (pathIndex >= 0 && pathIndex < _paths.Length)
            {
                var path = _paths[pathIndex];
                return Get(path + name + _postfix) ?? 
                       Get(path + _default + _postfix, true);
            }

            Assert(default, name);
            return default;
        }
        
        public Sprite LoadCurrency(string id)
        {
            if (Assert(id))
            {
                return default;
            }
            
            return Get(_paths[4] + _currency + id + _postfix, true);
        }
    }
}