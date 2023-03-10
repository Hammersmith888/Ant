using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Battle.Components
{
    [Battle, Unique]
    public class LastRoomIndexComponent : IComponent
    {
        public int Value;
    }
}