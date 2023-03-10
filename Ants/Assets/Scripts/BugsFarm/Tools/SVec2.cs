using System;
using UnityEngine;

[Serializable]
public struct SVec2
{
    public float x;
    public float y;

    public SVec2(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static implicit operator Vector2(SVec2 v) => new Vector2(v.x, v.y);
    public static implicit operator SVec2(Vector2 v) => new SVec2(v.x, v.y);
}

