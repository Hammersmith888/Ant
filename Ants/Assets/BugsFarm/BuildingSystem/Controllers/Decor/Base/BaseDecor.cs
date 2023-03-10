using BugsFarm.Services.SceneEntity;

namespace BugsFarm.BuildingSystem
{
    public abstract class BaseDecor : ISceneEntity
    {
        public string Id { get; }

        protected BaseDecor(string guid)
        {
            Id = guid;
        }

        public void Dispose(){}
    }
}