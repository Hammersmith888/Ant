using System;
using UnityEngine;


public struct GPos
{
    public GVert gVert { get; set; }
    public float t { get; set; }
    public GPos(GVert gVert, float t)
    {
        this.gVert = gVert;
        this.t = t;
    }
    public Vector2 GetPoint()
    {
        return gVert.GetPointAt(t);
    }
    public Vector2 GetNormal()
    {
        return gVert.GetPointNormalAt(t);
    }
    public static bool operator ==(GPos left, GPos right)
    {
        return left.gVert == right.gVert && Math.Abs(left.t - right.t) < Mathf.Epsilon;
    }
    public static bool operator !=(GPos left, GPos right)
    {
        return !(left == right);
    }
}

