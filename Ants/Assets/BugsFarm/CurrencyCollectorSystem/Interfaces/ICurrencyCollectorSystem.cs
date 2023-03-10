using System;
using UnityEngine;

namespace BugsFarm.CurrencyCollectorSystem
{
    public interface ICurrencyCollectorSystem
    {
        /// <summary>
        /// Collect currency with animation
        /// </summary>
        /// <param name="startPosition"> in world space</param>
        /// <param name="currencyID"> target currency type</param>
        /// <param name="totalCount"> total count to collect</param>
        /// <param name="onLeftCount"> int arg - count left [can be null] </param>
        /// <param name="onCompelte"> after all currency colleted event [can be null]</param>
        void Collect(Vector2 startPosition, 
                     string currencyID, 
                     int totalCount, 
                     bool useWorldPosition,
                     Action<int> onLeftCount = null,
                     Action onCompelte = null);

        void FlushCollect();
    }
}