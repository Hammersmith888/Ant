using System;
using System.Collections.Generic;
using UnityEngine;


public class WorldToGPos : MonoBehaviour
{
    public GPos GPos { get; private set; }

    private const int MaxSteps = 50;
    private const float MaxDeviation_fromMin = 1;
    private const float MaxDeviation_fromMax = 3;
    private static readonly Dictionary<int, int> _placeToGroup = new Dictionary<int, int>
    {
        // Grass
        [1] = 1,
        [2] = 1,
        [3] = 1,
        [4] = 1,
        [5] = 1,
        [6] = 1,
        [7] = 1,
        [8] = 1,
        [9] = 1,
        [36] = 1,           // used only for DigGround
        [37] = 1,           // used only for DigGround

        // Room 0
        [12] = 2,
        [38] = 2,
        // Room 1
        [10] = 2,
        [11] = 2,
        [21] = 2,
        [22] = 2,

        [13] = 3,
        [25] = 3,
        [32] = 3,

        [23] = 4,
        [24] = 4,
        [31] = 4,
        [35] = 4,
        [33] = 4,

        [14] = 5,
        [20] = 5,
        [26] = 5,

        [15] = 6,
        [27] = 6,
        [28] = 6,

        [16] = 7,
        [17] = 7,

        [18] = 8,
        [19] = 8,
        [29] = 8,
        [30] = 8,
        [34] = 8,
    };
    private GPos _leastEvil;
    private float _leastEvil_dev;

#pragma warning disable 0649

    [SerializeField] GVert _mirror_gVert;
    [SerializeField] float _mirror_t;

#pragma warning restore 0649

    public void Calc(int placeNum)
    {
        Vector2 pos = transform.position;


        GPos best = new GPos();
        float best_devY = Mathf.Infinity;
        int best_steps = 0;

        _leastEvil = new GPos();
        _leastEvil_dev = Mathf.Infinity;


        foreach (GVert road in Graph.PlacesGroups[_placeToGroup[placeNum]])
        {
            if ( road.Y_min - MaxDeviation_fromMin > pos.y || road.Y_max + MaxDeviation_fromMax < pos.y )
                continue;

            if ( road.P_xMin.x > pos.x || road.P_xMax.x < pos.x )
            {
                TestLeastEvil(road, 0);
                TestLeastEvil(road, 1);

                continue;
            }


            FindClosest_t_2(road, out float t, out Vector2 p, out int steps);

            float devY = Mathf.Abs(p.y - pos.y);

            if (devY < best_devY)
            {
                best.gVert = road;
                best.t = t;

                best_devY = devY;
                best_steps = steps;
            }
        }


        GPos = best.gVert ? best : _leastEvil;

        _mirror_gVert = GPos.gVert;
        _mirror_t = GPos.t;


        if (!GPos.gVert)
            throw new Exception($"Graph position not found for { transform.parent } / { transform }");

        // Print( best, best_steps );
    }

    private void TestLeastEvil(GVert road, float t)
    {
        Vector2 pos = transform.position;
        float dev = (pos - road.GetPointAt(t)).magnitude;

        if (dev < _leastEvil_dev)
        {
            _leastEvil = new GPos(road, t);
            _leastEvil_dev = dev;
        }
    }
    private void FindClosest_t_2(GVert road, out float t, out Vector2 p, out int steps)
    {
        float target = transform.position.x;

        t = Mathf.InverseLerp(road.P_xMin.x, road.P_xMax.x, target);
        t = road.RightDir ? t : 1 - t;

        p = road.GetPointAt(t);
        float dev = target - p.x;
        steps = 0;

        while (
                Mathf.Abs(dev) > .01f &&
                steps++ < MaxSteps
            )
        {
            t += dev / road.Length;
            p = road.GetPointAt(t);
            dev = target - p.x;
        }
    }


    // Zero links deprecated
    private void Print(GPos gPos, int steps)
    {
        Vector2 pos_Place = transform.position;
        Vector2 pos_gPos = gPos.GetPoint();
        Vector2 dev = pos_gPos - pos_Place;

        Debug.Log(
            "Best gPos:" +
            $" { gPos.gVert.name }" +
            $" (t = { gPos.t })" +
            $", pos: { pos_gPos.ToString("F4") }" +
            $", Place: { pos_Place.ToString("F4") }" +
            $", dev: { dev.ToString("F4") }" +
            $", steps: { steps }"
        );
    }
    private void FindClosest_t_1(GVert road, out float t, out Vector2 p, out int steps)
    {
        float target = transform.position.x;

        t = 0;
        float step = .5f;
        p = Vector2.zero;
        float dev = Mathf.Infinity;
        steps = 0;

        while (
                Mathf.Abs(dev) > .01f &&
                steps++ < MaxSteps
            )
        {
            t += Mathf.Sign(dev) * step;
            p = road.GetPointAt(t);
            dev = target - p.x;

            step /= 2;
            steps++;
        }
    }
}

