using System.Collections.Generic;
using UnityEngine;


public class Polyline
{
    public float Length { get; private set; }
    public float Y_min { get; private set; }
    public float Y_max { get; private set; }

    private const float SegmentLength = Constants.Polyline_SegmentLength;
    private const float resolution = Constants.Polyline_Resolution;
    private readonly List<Vector2> _points = new List<Vector2>();

    public void Init(BezierCurve bezier)
    {
        int n_segments = Mathf.RoundToInt(bezier.length / SegmentLength);

        float step = 1f / (n_segments * resolution);
        _points.Capacity = n_segments * 2;

        float t = 0;
        Vector2? prev = null;

        Y_min = Mathf.Infinity;
        Y_max = Mathf.NegativeInfinity;

        while (t < .999f)
        {
            Vector2 next = bezier.GetPointAt(t);
            float dist = prev.HasValue ? Vector2.Distance(prev.Value, next) : 0;

            if ( !prev.HasValue || dist > SegmentLength )
            {
                _points.Add(next);
                Length += dist;
                Y_min = Mathf.Min(Y_min, next.y);
                Y_max = Mathf.Max(Y_max, next.y);

                prev = next;
            }

            t += step;
        }
    }
    public void Stretch(Vector2? p0, Vector2? p1)
    {
        // Original
        Vector2 origin0 = _points[0];
        Vector2 origin1 = _points[_points.Count - 1];
        Vector2 originNormal = Vector2.Perpendicular(origin1 - origin0).normalized;
        float oLength = Vector2.Distance(origin0, origin1);

        // New
        Vector2 new0 = p0 ?? origin0;
        Vector2 new1 = p1 ?? origin1;
        Vector2 newNormal = Vector2.Perpendicular(new1 - new0).normalized;
        float nLength = Vector2.Distance(new0, new1);

        for (int i = 0; i < _points.Count; i++)
        {
            Vector2 point = _points[i];
            Vector2 direction = point - origin0;
            float h = Vector2.Dot(direction, originNormal);
            Vector2 oP = point - (h * originNormal);                                // Projection
            float t = Vector2.Distance(origin0, oP) / oLength;

            Vector2 nP = Vector2.Lerp(new0, new1, t);           // Projection
            Vector2 n = nP + (h * newNormal);

            _points[i] = n;
        }

        Length *= nLength / oLength;
    }
    public Vector2 GetPointAt(float t)
    {
        GetPointsByT(ref t, out int index0, out int index1, out Vector2 point0, out Vector2 point1);

        float pointT0 = index0 / (_points.Count - 1f); // 0.489795f
        float pointT1 = index1 / (_points.Count - 1f); // 0.510204f

        float centerBy_t = Mathf.InverseLerp(pointT0, pointT1, t);

        return Vector2.Lerp(point0, point1, centerBy_t);
    }
    public Vector2 GetPointNormalAt(float t)
    {
        GetPointsByT(ref t, out int i0, out int i1, out Vector2 p0, out Vector2 p1);

        Vector2 normal = Vector2.Perpendicular(p1 - p0);

        normal.y = Mathf.Abs(normal.y);

        return normal;
    }
    private void GetPointsByT(ref float t, out int index0, out int index1, out Vector2 point0, out Vector2 point1)
    {
        t = Mathf.Clamp01(t) * .999f; //t= 0.5f,  exit = 0.4995f

        index0 = Mathf.FloorToInt(t * (_points.Count - 1));//24
        index1 = index0 + 1; //25

        point0 = _points[index0];
        point1 = _points[index1];
    }
}

