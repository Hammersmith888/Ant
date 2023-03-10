using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Battle.Components
{
    [Battle]
    public class RoomIndexComponent : IComponent
    {
        [PrimaryEntityIndex] public int Value;
    }
}