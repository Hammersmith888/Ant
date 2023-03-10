using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Vector3Extensions
{
    public static Vector3 GetCenter(this List<Vector3> vectors)
    {
        Vector3 sum = Vector3.zero;
        if (vectors == null || vectors.Count == 0)
        {
            return sum;
        }

        foreach (Vector3 vec in vectors)
        {
            sum += vec;
        }
        return sum / vectors.Count;
    }
}