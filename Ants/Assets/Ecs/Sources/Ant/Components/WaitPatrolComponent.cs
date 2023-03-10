using Entitas;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class WaitPatrolComponent : IComponent
    {
        public float Time;
        public float TimeMax;
    }
}