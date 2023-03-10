using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class ColorExtensions
{
    public static Color Change_A(this Color color, float a)
    {
        color.a = a;
        return color;
    }

    public static Color Change_RGB(this Color color, float r, float g, float b)
    {
        color.r = r;
        color.g = g;
        color.b = b;

        return color;
    }
    //
    public static Color Change_RGB(this Color color, float r, float g, float b, float a)
    {
        color.r = r;
        color.g = g;
        color.b = b;
        color.a = a;

        return color;
    }
}
