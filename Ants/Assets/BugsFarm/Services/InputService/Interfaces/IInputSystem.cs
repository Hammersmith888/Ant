using System;
using UnityEngine;

namespace BugsFarm.Services.InputService
{
    public interface IInputSystem
    {
       event Action OnInput;
       Vector2 Position { get; }
       TouchPhase Phase { get; }
    }
}