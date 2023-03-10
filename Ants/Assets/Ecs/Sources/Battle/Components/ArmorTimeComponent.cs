using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Battle.Components
{
    [Battle, Unique]
    public class ArmorTimeComponent : IComponent
    {
        public float Value;
    }
}