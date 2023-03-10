using UnityEngine;

namespace BugsFarm.AssetLoaderSystem
{
    public class PrefabLoader : BaseLoader<GameObject>
    {
        private readonly string[] _paths = 
        {
            "Prefabs/",
            "Prefabs/Buildings/",
            "Prefabs/Decor/",
            "Prefabs/Food/",
            "Prefabs/Misc/",
            "Prefabs/PlaceIDs/",
            "Prefabs/Units/",
            "Prefabs/Rooms/",
            "Prefabs/Chests/",
            "Prefabs/LeafHeaps/",
            "Prefabs/Cameras/",
        };
        
        public GameObject Load(string name)
        {
            if (Assert(name))
            {
                return default;
            }
            
            foreach (var path in _paths)
            {
                var loadObj = Get(path + name);
                if (loadObj)
                {
                    return loadObj;
                }
            }

            Assert(default, name);
            return default;
        }
    }
}