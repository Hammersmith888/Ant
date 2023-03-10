using UnityEngine;

namespace BugsFarm.AssetLoaderSystem
{
    public class SoundLoader : BaseLoader<AudioClip>
    {
        public bool TryLoad(string path, out AudioClip clip, bool catchError = false)
        {
            return clip = Get(path, catchError);
        }
    }
}