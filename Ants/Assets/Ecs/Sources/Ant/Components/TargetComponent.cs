using Ecs.Managers;
using Ecs.Utils;
using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class TargetComponent : IComponent
    {
        public Transform Transform;
        public Uid Uid;
    }
}