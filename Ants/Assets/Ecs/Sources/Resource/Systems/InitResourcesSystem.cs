using Entitas;

namespace Ecs.Sources.Resource.Systems
{
    public class InitResourcesSystem : IInitializeSystem
    {
        private readonly ResourceContext _resource;

        public InitResourcesSystem(ResourceContext resource)
        {
            _resource = resource;
        }

        public void Initialize()
        {
            _resource.ReplaceFoodStock(1000);
        }
    }
}