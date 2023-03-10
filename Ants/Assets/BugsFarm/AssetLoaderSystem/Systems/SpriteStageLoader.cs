using UnityEngine;

namespace BugsFarm.AssetLoaderSystem
{
    public class SpriteStageLoader : BaseLoader<Sprite>
    {
        private const string _postfix = "_stage";
        
        public Sprite Load(string basePath, string name, int stage, bool warrning = false)
        {
            return Get(basePath + name + "_"+ stage + _postfix, warrning);
        }
    }
}