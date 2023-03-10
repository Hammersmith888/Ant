using Ecs.Utils;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Custom.Components
{
    [Ant, Battle]
    public class UidComponent : IComponent
    {
        [PrimaryEntityIndex] public Uid Value;
    }
}