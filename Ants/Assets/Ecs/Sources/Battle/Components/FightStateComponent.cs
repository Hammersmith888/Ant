using BugsFarm.Model.Enum;
using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Battle.Components
{
    [Battle, Unique]
    public class FightStateComponent : IComponent
    {
        public EFightState Value;
    }
}