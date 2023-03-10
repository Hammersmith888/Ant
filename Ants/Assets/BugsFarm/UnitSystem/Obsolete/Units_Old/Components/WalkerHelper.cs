using System.Collections.Generic;
using BugsFarm.UnitSystem.Obsolete.Components;
using UnityEngine;

namespace BugsFarm.UnitSystem.Obsolete
{
    public static class WalkerHelper
    {
        public static float CalcSpeedMul(Walker walker)
        {
            float speedMul = 1;


            GPos gPos = walker.gPos;
            GVert gVert = gPos.gVert;


            // Текущая кривая - gVert
            if (Occupied.Walkers.ContainsKey(gVert))
            {
                foreach (var other in Occupied.Walkers[gVert])
                {
                    if (other.Key != walker) // Не является самим собой
                    {
                        bool isSameDirection = other.Value.IsForward == walker.CurStep.IsForward; // Идет в то же направление
                        bool isNotTooClose = Mathf.Abs(gPos.t - other.Value.t) > .001f;           // Не слишком близко
                        bool isGreaterID = walker.ID > other.Key.ID;                              // Имеет больший идентификатор ID (Если муравьи находятся слишком близко, то низкие ID могут свободно проходить)
                        bool isAheadOfMe = other.Value.IsForward == gPos.t < other.Value.t;       // Впереди меня

                        if (isSameDirection && (isNotTooClose || isGreaterID) && isAheadOfMe)
                        {
                            CalcSpeedMul(walker, other, ref speedMul);
                        }
                    }
                }
            }

            // Ближайший или соседние - gVerts
            if (!walker.IsLadder) // (!) Важно: возможность спуститься с лестницы всегда должна быть (!)
            {
                foreach (GEdge edge1 in Graph.Instance.AdjacentEdges(gVert))
                {
                    GVert joint = edge1.Other(gVert);

                    if (walker.CurStep.IsForward != (joint == gVert.Joint_t1))       // Если НЕ идет в направлении стыка -joint 
                        continue;

                    foreach (GEdge edge2 in Graph.Instance.AdjacentEdges(joint))
                    {
                        GVert adjacent = edge2.Other(joint);

                        if (adjacent == gVert || !Occupied.Walkers.ContainsKey(adjacent))
                            continue;

                        foreach (var other in Occupied.Walkers[adjacent])
                        {
                            if (other.Key != walker) // Не является самим собой
                            {
                                if (other.Value.IsForward == (joint == adjacent.Joint_t0)) // Идет в то же направление
                                {
                                    CalcSpeedMul(walker, other, ref speedMul);
                                }
                            }
                        }
                    }
                }
            }

            return speedMul;
        }
        private static void CalcSpeedMul(Walker walker, KeyValuePair<Walker, StepInfo> other, ref float speedMul)
        {
            // Другой ждет, пока лестница освободится и не последний шаг
            if (other.Key.WaitForLadders && !walker.Path.IsLastStep())
            {
                bool isHaveOtherLadder = walker.NextStep.gVert != other.Key.NextStep.gVert; // идти по другой лестнице, а не по той, которую ждет другой муравей
                bool canJumpFromLadder = walker.CanJump && walker.NextStep.IsLadderDown;    // может спрыгнуть с лестницы

                if (isHaveOtherLadder || walker.DontCheckLadders || canJumpFromLadder)
                {
                    return;
                }
            }

            float gap = Vector2.Distance(walker.Agent.position, other.Value.Position);
            float mul = Mathf.InverseLerp(Constants.SpeedMul_GapMin, Constants.SpeedMul_GapMax, gap);

            speedMul = Mathf.Min(speedMul, mul);
        }
    }
}

