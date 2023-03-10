using Ecs.Managers;

namespace Ecs.Sources.Battle.Utils
{
    public static class BattleExtensions
    {
        public static BattleEntity CreateRoom(this BattleContext battle)
        {
            var entity = battle.CreateEntity();

            entity.isRoom = true;
            entity.AddUid(UidGenerator.Next());

            return entity;
        }
    }
}