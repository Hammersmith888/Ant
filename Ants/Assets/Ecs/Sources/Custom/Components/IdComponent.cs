using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Custom.Components
{
    [Ant]
    public class IdComponent : IComponent
    {
        [PrimaryEntityIndex] public int Value;
    }
}