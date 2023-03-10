using System;
using UnityEngine;

namespace BugsFarm.Services.InputService
{
    public interface ISwipeSystem
    {
        event Action<Vector2> OnSwipe;
    }
}