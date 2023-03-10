using UnityEngine;

public readonly struct GStep
{
    public GStep(GVert gVert, float t0, float t1 = 1)
    {
        this.gVert = gVert;
        this.t0 = t0;
        this.t1 = t1;


        Delta_t = t1 - t0;
        IsForward = t1 > t0;

        PointAt_t0 = gVert.GetPointAt(t0);
        PointAt_t1 = gVert.GetPointAt(t1);

        IsLadder = gVert.IsLadder;
        IsLadderDown = IsLadder && PointAt_t1.y < PointAt_t0.y;
    }


    // Calculated
    public GVert gVert { get; }
    public float t0 { get; }
    public float t1 { get; }

    public float Delta_t { get; }
    public bool IsForward { get; }

    public Vector2 PointAt_t0 { get; }
    public Vector2 PointAt_t1 { get; }

    public bool IsLadder { get; }
    public bool IsLadderDown { get; }
}

