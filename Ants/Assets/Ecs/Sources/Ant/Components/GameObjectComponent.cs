using Entitas;
using UnityEngine;

namespace Ecs.Sources.Ant.Components
{
    [Ant]
    public class GameObjectComponent : IComponent
    {
        public GameObject Value;
    }
}