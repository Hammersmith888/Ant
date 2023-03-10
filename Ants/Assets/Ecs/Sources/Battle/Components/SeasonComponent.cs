using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Battle.Components
{
    [Battle, Unique]
    public class SeasonComponent : IComponent
    {
        public int Value;
    }
}