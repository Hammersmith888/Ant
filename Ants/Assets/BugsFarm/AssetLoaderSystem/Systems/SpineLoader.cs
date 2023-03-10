using Spine.Unity;

namespace BugsFarm.AssetLoaderSystem
{
    public class SpineLoader : BaseLoader<SkeletonDataAsset>
    {
        private const string _stagetext = "_Stage_";
        private const string _postfix = "_SkeletonData";
        
        public SkeletonDataAsset LoadStage(string basePath, string name, int stage)
        {
            return Get(basePath + name + _stagetext + stage + _postfix, true);
        }
        
        public SkeletonDataAsset Load(string basePath, string name)
        {
            return Get(basePath + name + _postfix, true);
        }
    }
}