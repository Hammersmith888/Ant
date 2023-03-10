using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class ChestRoomIndexComponent : IComponent
    {
        [PrimaryEntityIndex] public int Value;
    }
}