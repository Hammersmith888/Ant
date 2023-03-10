using Entitas;
using Entitas.CodeGeneration.Attributes;

namespace Ecs.Sources.Resource.Components
{
    [Resource, Unique]
    public class FoodStockComponent : IComponent
    {
        public int Value;
    }
}