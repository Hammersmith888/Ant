using System.Collections.Generic;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete.Components
{
    public static class Occupied
    {
        public static readonly
            Dictionary<Walker, Vector2> Targets = new Dictionary<Walker, Vector2>();

        public static readonly
            Dictionary<GVert, Dictionary<Walker, StepInfo>> Walkers = new Dictionary<GVert, Dictionary<Walker, StepInfo>>();

        /*
		Walkers updates frequently, so we have to maintain _walkerGVerts
		Otherwise there will be a lot of FOREACH loops each frame to find specific walker
	*/
        private static readonly
            Dictionary<Walker, GVert> _walkerGVerts = new Dictionary<Walker, GVert>();

        public static readonly
            Dictionary<GVert, Dictionary<Walker, bool>> Ladders = new Dictionary<GVert, Dictionary<Walker, bool>>();

        public static void Clear()
        {
            Targets.Clear();

            foreach (var gVert in Walkers)
                gVert.Value.Clear();

            _walkerGVerts.Clear();

            foreach (var gVert in Ladders)
                gVert.Value.Clear();
        }
        public static void Target_Occupy(Walker walker, GPos gPos)
        {
            Targets[walker] = gPos.GetPoint();
        }
        public static void Target_Free(Walker walker)
        {
            Targets.Remove(walker);
        }
        public static void WalkPos_Occupy(Walker walker, GStep step, Vector2 pos, float t)
        {
            GVert gVert = step.gVert;

            if ( _walkerGVerts.TryGetValue(walker, out GVert old) && old != gVert )
                RemoveWalker(walker);

            if (!Walkers.TryGetValue(gVert, out var walkers))
                Walkers[gVert] = walkers  = new Dictionary<Walker, StepInfo>() ;

            walkers[walker] = new StepInfo() { Position = pos, t = t, IsForward = step.IsForward };
            _walkerGVerts[walker] = gVert;
        }
        public static void WalkPos_Free(Walker walker)
        {
            // It is possible if the path was very short, so whole its length was consumed during only 1 frame and walker position was not booked.
            if (!_walkerGVerts.ContainsKey(walker))
                return;

            RemoveWalker(walker);
        }
        public static bool Ladder_IsOccupied(GStep step)
        {
            if (!Ladders.ContainsKey(step.gVert))
                return false;

            foreach (var walker in Ladders[step.gVert])
                if (walker.Value != step.IsForward)
                    return true;

            return false;
        }
        public static void Ladder_Occupy(Walker walker, GStep step)
        {
            if (!Ladders.TryGetValue(step.gVert, out var walkers))
                Ladders[step.gVert] = walkers
                        = new Dictionary<Walker, bool>()
                    ;

            walkers[walker] = step.IsForward;
        }
        public static void Ladder_Free(Walker walker, GVert gVert)
        {
            Ladders.TryGetValue(gVert, out var walkers);

            walkers?.Remove(walker);            // walkers can be NULL in case we've just deserialized ant who was on a ladder
        }
        public static void Ladder_FreeAll(Walker walker)
        {
            // This should not happen too often, so FOREACH is OK
            foreach (var walkers in Ladders)
                walkers.Value.Remove(walker);
        }

        private static void RemoveWalker(Walker walker)
        {
            GVert gVert = _walkerGVerts[walker];

            Walkers[gVert].Remove(walker);
            _walkerGVerts.Remove(walker);
        }
    }

    public struct StepInfo
    {
        /// <summary>
        /// Позиция
        /// </summary>
        public Vector2 Position { get; set; }
        /// <summary>
        /// Параметр t
        /// </summary>
        public float t { get; set; }
        /// <summary>
        /// Является ли шаг в направление цели
        /// </summary>
        public bool IsForward { get; set; }
    }
}