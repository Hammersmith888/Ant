using BugsFarm.Services.SceneEntity;
using Zenject;

namespace BugsFarm.BuildingSystem
{
    public class RelaxationRoom : ISceneEntity, IInitializable
    {
        public string Id { get; }
        public RelaxationRoom(string guid)
        {
            Id = guid;
        }

        public void Initialize()
        {

        }
        
        public void Dispose()
        {

        }
    }
}