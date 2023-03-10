using System;
using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public interface ICurrencyAnimation
    {
        /// <summary>
        /// Collect currency with animation
        /// </summary>
        /// <param name="startPosition"> in world space</param>
        /// <param name="targetPosition"> in world space</param>
        /// <param name="itemTarget"> target item to animate</param>
        /// <param name="targetRenderer">target transformation camera</param>
        /// <param name="onComplete"> after all currency colleted event [can be null]</param>
        void Animate(Vector2 startPosition, 
                     Vector2 targetPosition, 
                     bool useWorldPosition,
                     ICurrencyAnimationItem itemTarget, 
                     Camera targetRenderer,
                     Action onComplete);
    }
}