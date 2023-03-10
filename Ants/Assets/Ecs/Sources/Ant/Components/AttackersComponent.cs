using Ecs.Utils;
using Entitas;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class AttackersComponent : IComponent
    {
        public Uid[] Uids;
    }
}