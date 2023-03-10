using Ecs.Managers;

namespace Ecs.Sources.Ant.Utils
{
    public static class AntExtensions
    {
        public static AntEntity CreateAnt(this AntContext ant)
        {
            var entity = ant.CreateEntity();

            entity.isAnt = true;
            entity.AddUid(UidGenerator.Next());
            
            return entity;
        }
    }
}