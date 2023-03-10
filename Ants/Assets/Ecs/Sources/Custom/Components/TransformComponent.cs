using Entitas;
using UnityEngine;

namespace Ecs.Sources.Custom.Components
{
    [Ant]
    public class TransformComponent : IComponent
    {
        public Transform Value;
    }
}