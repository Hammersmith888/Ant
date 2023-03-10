using Entitas;
using Entitas.CodeGeneration.Attributes;
using UnityEngine;

namespace Ecs.Sources.Battle.Components
{
    [Battle, Unique]
    public class MoveCameraComponent : IComponent
    {
        public Vector3 From;
        public Vector3 To;
        public float StartTime;
    }
}