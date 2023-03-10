using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class BooleanExtension
{
    public static int ToInt(this bool x)
    {
        return x ? 1 : 0;
    }
}