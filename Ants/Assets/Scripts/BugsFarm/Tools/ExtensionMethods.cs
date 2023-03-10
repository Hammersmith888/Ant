using System;
using UnityEngine;


public enum Axis { X, Y, Z }


static class ExtensionMethods
{
    public static Vector2Int Abs(this Vector2Int v) { return new Vector2Int(Mathf.Abs(v.x), Mathf.Abs(v.y)); }
    public static Vector2 Abs(this Vector2 v) { return new Vector2(Mathf.Abs(v.x), Mathf.Abs(v.y)); }
    public static Vector3 Abs(this Vector3 v) { return new Vector3(Mathf.Abs(v.x), Mathf.Abs(v.y), Mathf.Abs(v.z)); }


    public static Vector2Int Swap(this Vector2Int v)
    {
        return new Vector2Int(v.y, v.x);
    }


    public static int Sum(this Vector2Int v) { return v.x + v.y; }
    public static float Sum(this Vector2 v) { return v.x + v.y; }

    public static float Min(this Vector2 v) { return Mathf.Min(v.x, v.y); }
    public static float Max(this Vector2 v) { return Mathf.Max(v.x, v.y); }


    /// <summary>
    /// Образовывает 2D прямоугольник на основе двух точек. Проверяет находится ли точка в этом прямоугольнике. 
    /// </summary>
    public static bool Contains(this Vector3 v, Vector2 max, Vector2 min)
    {
        if (v == (Vector3)max || v == (Vector3)min) return true;
        
        var maxX = Mathf.Max(max.x, min.x);
        var maxY = Mathf.Max(max.y, min.y);
        
        var minX = Mathf.Min(max.x, min.x);
        var minY = Mathf.Min(max.y, min.y);

        if (v.x > maxX || v.x < minX) return false;
        if (v.y > maxY || v.y < minY) return false;
        
        return true;
    }

    public static float GetAxis(this Vector3 v, Axis axis)
    {
        return
            axis == Axis.X ?
            v.x :

            axis == Axis.Y ?
            v.y :

            v.z
        ;
    }

    public static Vector3 SetAxis(this Vector3 v, Axis axis, float value)
    {
        return new Vector3(
            axis == Axis.X ? value : v.x,
            axis == Axis.Y ? value : v.y,
            axis == Axis.Z ? value : v.z
        );
    }


    public static Vector3 SetXY(this Vector3 v, float x, float y)
    {
        v.x = x;
        v.y = y;

        return v;
    }


    public static Vector3 SetXY(this Vector3 v, Vector2 xy)
    {
        return v.SetXY(xy.x, xy.y);
    }


    public static Vector3 SetXZ(this Vector3 v, Vector3 xz)
    {
        v.x = xz.x;
        v.z = xz.z;

        return v;
    }


    public static Vector3 SetX(this Vector3 v, float x) { v.x = x; return v; }
    public static Vector3 SetY(this Vector3 v, float y) { v.y = y; return v; }
    public static Vector3 SetZ(this Vector3 v, float z) { v.z = z; return v; }

    public static Vector2 SetX(this Vector2 v, float x) { v.x = x; return v; }
    public static Vector2 SetY(this Vector2 v, float y) { v.y = y; return v; }
    public static Vector3 Multipy (this  Vector3 a, Vector3 b)
    {
        a.Scale(b);
        return a;
    }


    #region Transform

    public static void SetXY(this Transform transform, Vector2 xy) => transform.position = transform.position.SetXY(xy);

    public static void SetX(this Transform transform, float x) => transform.position = transform.position.SetX(x);
    public static void SetY(this Transform transform, float y) => transform.position = transform.position.SetY(y);
    public static void SetZ(this Transform transform, float z) => transform.position = transform.position.SetZ(z);

    #endregion


    /*
		https://stackoverflow.com/a/643438/4830242
	*/
    public static T Next<T>(this T src) where T : struct
    {
        if (!typeof(T).IsEnum)
            throw new ArgumentException(String.Format("Argument {0} is not an Enum", typeof(T).FullName));

        T[] arr = (T[])Enum.GetValues(src.GetType());
        int j = Array.IndexOf<T>(arr, src) + 1;

        return arr.Length == j ? arr[0] : arr[j];
    }


    // https://extensionmethod.net/csharp/timespan/round-to-nearest-timespan
    public static TimeSpan RoundToNearest(this TimeSpan a, TimeSpan roundTo)
    {
        long ticks = (long)(Math.Round(a.Ticks / (double)roundTo.Ticks) * roundTo.Ticks);

        return new TimeSpan(ticks);
    }

    /// <summary>
    /// Проверяет является объект по умолчанию или null
    /// </summary>
    public static bool IsNullOrDefault<T>(this T value)
    {
        return value == null || value.Equals(default(T));
    }
}

