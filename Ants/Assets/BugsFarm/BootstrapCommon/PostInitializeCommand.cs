using BugsFarm.Services.BootstrapService;
using BugsFarm.Services.SceneEntity;
using BugsFarm.Services.StorageService;

namespace BugsFarm.BootstrapCommon
{
    public class PostInitializeCommand : Command
    {
        private readonly IStorage<ISceneEntity> _sceneEntityStorage;

        public PostInitializeCommand(IStorage<ISceneEntity> sceneEntityStorage)
        {
            _sceneEntityStorage = sceneEntityStorage;
        }

        public override void Do()
        {
            foreach (var sceneEntity in _sceneEntityStorage.Get())
            {
                if (sceneEntity is IPostInitializable postSceneEntity)
                {
                    postSceneEntity.OnPostInitialize();
                }
            }
            
            base.Do();
        }
    }
}