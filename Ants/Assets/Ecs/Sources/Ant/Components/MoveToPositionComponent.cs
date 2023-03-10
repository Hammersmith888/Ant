using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class MoveToPositionComponent : IComponent
    {
        public Vector3 Position;
    }
}