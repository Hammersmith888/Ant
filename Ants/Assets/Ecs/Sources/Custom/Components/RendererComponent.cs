using Entitas;
using UnityEngine;

namespace Ecs.Sources.Custom.Components
{
    [Ant]
    public class RendererComponent : IComponent
    {
        public Renderer Value;
    }
}