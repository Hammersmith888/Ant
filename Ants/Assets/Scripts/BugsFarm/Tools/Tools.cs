using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Random = UnityEngine.Random;


public static class Tools
{
    static readonly NumberFormatInfo _nfi = (NumberFormatInfo)CultureInfo.InvariantCulture.NumberFormat.Clone();


    public static string ThSep(float number)
    {
        return ThSep(Mathf.RoundToInt(number));
    }
    public static string ThSep(int number)
    {
        // https://stackoverflow.com/a/17527989/4830242

        // _nfi.NumberGroupSeparator		= " ";
        _nfi.NumberGroupSeparator = ",";
        // _nfi.NumberGroupSeparator		= "\u00A0";

        return number.ToString("N0", _nfi);
    }


    public static float ScaleFactor(CanvasScaler canvasScaler)
    {
        const float kLogBase = 2;
        
        var screenSize = new Vector2(Screen.width, Screen.height);
        var refRes = canvasScaler.referenceResolution;
        var match = canvasScaler.matchWidthOrHeight;

        var logWidth = Mathf.Log(screenSize.x / refRes.x, kLogBase);
        var logHeight = Mathf.Log(screenSize.y / refRes.y, kLogBase);
        var logWeightedAverage = Mathf.Lerp(logWidth, logHeight, match);
        var scaleFactor = Mathf.Pow(kLogBase, logWeightedAverage);

        return scaleFactor;
    }


    public static float UnsafeTop_Ref(CanvasScaler canvasScaler)
    {
        // Notation: "ref" is the closest match to the ReferenceResolution

        var unsafeTopScr = Screen.height - Screen.safeArea.y - Screen.safeArea.height;
        var refToScr = ScaleFactor(canvasScaler);
        var scrToRef = 1 / refToScr;
        return unsafeTopScr * scrToRef;
    }


    #region RectTransform

    public static void MinMax_1RT(RectTransform rt, out Vector2 min, out Vector2 max)
    {
        min = Vector2.positiveInfinity;
        max = Vector2.negativeInfinity;

        MinMax_1RT(ref min, ref max, rt);
    }


    static void MinMax_1RT(ref Vector2 min, ref Vector2 max, RectTransform rt)
    {
        var corners = new Vector3[4];
        rt.GetWorldCorners(corners);

        foreach (var corner in corners)
        {
            min = Vector2.Min(min, corner);
            max = Vector2.Max(max, corner);
        }
    }


    static void MinMax_All(RectTransform rt, out Vector2 min, out Vector2 max, bool countChildren)
    {
        min = Vector2.positiveInfinity;
        max = Vector2.negativeInfinity;

        MinMax_1RT(ref min, ref max, rt);

        if (countChildren)
        {
            RectTransform[] children = rt.gameObject.GetComponentsInChildren<RectTransform>(false);

            foreach (var child in children)
                MinMax_1RT(ref min, ref max, child);
        }
    }


    /*
		(!) Works only for Canvas Render Mode = Screen Space - Camera
	*/
    public static Vector3 Pos_OutOfScr(RectTransform rt, Vector2Int dir)
    {
        MinMax_All(rt, out var min, out var max, true);

        Vector2 camPos = Camera.main.transform.position;
        Vector2 cam_halfSize = CamSize() / 2;

        Vector2 pos = rt.position;

        Vector2 shift_Extents = dir.Sum() > 0 ? pos - min : max - pos;
        Vector2 addon = cam_halfSize.y / 100 * Vector2.one;

        Vector3 pos_3d =
            Vector2.Scale(pos, dir.Swap().Abs()) +      // keep other axis coord
            Vector2.Scale(camPos, dir.Abs()) +      // get anim axis camera coord
            Vector2.Scale(cam_halfSize + addon + shift_Extents, dir)                            // shift along anim axis
        ;

        pos_3d.z = rt.position.z;

        return pos_3d;
    }


    public static RectTransform CopyRectTransform(RectTransform src, string name = "RT Copy")
    {
        RectTransform rt = new GameObject(name, typeof(RectTransform)).GetComponent<RectTransform>();

        rt.transform.SetParent(src.transform.parent, false);

        rt.anchorMin = src.anchorMin;
        rt.anchorMax = src.anchorMax;
        rt.anchoredPosition = src.anchoredPosition;
        rt.sizeDelta = src.sizeDelta;

        return rt;
    }

    #endregion


    public static Vector2 CamSize()
    {
        return Camera.main.orthographicSize * new Vector2(Camera.main.aspect, 1) * 2;
    }


    /*
		https://stackoverflow.com/a/9382468/4830242

		Alternative - via SymbolExtensions class:
			https://stackoverflow.com/a/9382432/4830242
	*/
    public static string MethodName(Action a)
    {
        return a.Method.Name;
    }


    /*
		https://stackoverflow.com/a/9382501/4830242
	*/
    public static MethodInfo GetMethodInfo<T>(Expression<Action<T>> expression)
    {
        if (expression.Body is MethodCallExpression member)
            return member.Method;

        throw new ArgumentException("Expression is not a method", "expression");
    }


    public static Vector2 Size(SpriteRenderer spriteRenderer)
    {
        var sprite = spriteRenderer.sprite;
        var textureSize = new Vector2(sprite.texture.width, sprite.texture.height);
        var size = spriteRenderer.transform.lossyScale * textureSize / sprite.pixelsPerUnit;

        return size;
    }


    #region Random

    public static int RandomWeightedIndex(float[] weights)
    {
        // https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/

        if (weights == null || weights.Length == 0) return -1;

        float w;
        float total = 0;
        int i;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];

            if (Single.IsPositiveInfinity(w)) return i;

            if (w >= 0f && !Single.IsNaN(w))
                total += w;
        }

        float r = Random.value;
        float s = 0;

        for (i = 0; i < weights.Length; i++)
        {
            w = weights[i];

            if (Single.IsNaN(w) || w <= 0) continue;

            s += w / total;

            if (s >= r) return i;
        }

        return -1;
    }


    public static bool RandomWeighted<T>(List<(T, float)> weights, out T random)
    {
        // https://forum.unity.com/threads/random-numbers-with-a-weighted-chance.442190/

        random = default;

        if (weights == null || weights.Count == 0) return false;

        float w;
        float total = 0;
        int i;

        for (i = 0; i < weights.Count; i++)
        {
            (random, w) = weights[i];

            if (Single.IsPositiveInfinity(w)) return true;

            if (w >= 0f && !Single.IsNaN(w))
                total += w;
        }

        float r = Random.value;
        float s = 0;

        for (i = 0; i < weights.Count; i++)
        {
            (random, w) = weights[i];

            if (Single.IsNaN(w) || w <= 0) continue;

            s += w / total;

            if (s >= r) return true;
        }

        return false;
    }


    public static T RandomWeighted<T>(List<(T, float)> weights)
    {
        RandomWeighted(weights, out T result);

        return result;
    }


    public static bool RandomBool() => Random.Range(0, 2) == 0;
    
    public static T RandomItem<T>(List<T> items) => items[Random.Range(0, items.Count)];
    
    public static T RandomItem<T>(params T[] args)
    {
        return args == null || args.Length <= 0 ? default : args[Random.Range(0, args.Length)];
    }
    public static T RandomItem<T>(out int index , params T[] args)
    {
        index = Random.Range(0, args.Length);
        return args.Length <= 0 ? default : args[index];
    }

    public static int RandomRange(Vector2Int range) => Random.Range(range.x, range.y);
    public static float RandomRange(Vector2 range) => Random.Range(range.x, range.y);
    public static Vector2 RangeRandom(this Vector2 range)
    {
        return new Vector2(RandomRange(range), RandomRange(range));
    }

    /*
		Shuffle (Fisher–Yates):
			https://en.wikipedia.org/wiki/Fisher%E2%80%93Yates_shuffle
	*/
    public static void Shuffle_FisherYates<T>(List<T> list)
    {
        if (list == null || list.Count == 0) return;
        var n = list.Count;

        for (var i = 0; i < n - 1; i++)
        {
            var j = Random.Range(i, n);

            var tmp = list[i];
            list[i] = list[j];
            list[j] = tmp;
        }
    }
    public static IEnumerable<T> Shuffle_FisherYates<T>(IEnumerable<T> array)
    {
        if (array == null || !array.Any()) return new T[0];
        var arr = array.ToArray();
        var n = arr.Length;

        for (var i = 0; i < n - 1; i++)
        {
            var j = Random.Range(i, n);

            var tmp = arr[i];
            arr[i] = arr[j];
            arr[j] = tmp;
        }
        return arr;
    }
    #endregion


    public static void Log(
            string text = "",

            [CallerFilePath] string file = "",
            [CallerMemberName] string member = "",
            [CallerLineNumber] int line = 0
        )
    {
        // https://stackoverflow.com/a/16295463/4830242

        Debug.LogFormat("{0}_{1}({2}): {3}", Path.GetFileName(file), member, line, text);
    }


    static List<RaycastResult> _raycastResults = new List<RaycastResult>();

    static bool _IsPointerOverUIObject()
    {
        // https://answers.unity.com/questions/1115464/ispointerovergameobject-not-working-with-touch-inp.html?_ga=2.108836996.765519434.1590144612-53114821.1572611642#answer-1115473

        PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
        eventDataCurrentPosition.position = Input.mousePosition;

        _raycastResults.Clear();
        EventSystem.current.RaycastAll(eventDataCurrentPosition, _raycastResults);

        return _raycastResults.Count > 0;
    }


    static bool _IsPointerOverGameObject()
    {
        // http://answers.unity.com/answers/1643456/view.html

        // Check mouse
        if (EventSystem.current.IsPointerOverGameObject())
            return true;

        // Check touches
        for (int i = 0; i < Input.touchCount; i++)
        {
            Touch touch = Input.GetTouch(i);

            if ( touch.phase == TouchPhase.Began &&
                 EventSystem.current.IsPointerOverGameObject(touch.fingerId))
                return true;
        }

        return false;
    }


    public static bool IsPointerOverGameObject()
    {
        return _IsPointerOverGameObject() || _IsPointerOverUIObject();          // Not working on Xiaomi MI 5
    }


    public static T SingletonPattern<T>(T @this, T instance) where T : class
    {
        if (instance != null)
            throw new Exception($"Singleton pattern violation: instance of class { @this.GetType().Name } already exists!");

        return @this;
    }


    public static Vector2 Mouse_w() => Camera.main.ScreenToWorldPoint(Input.mousePosition);


    public static int Ceil(int x, int y)
    {
        return (x - 1) / y + 1;
    }


    public static double Clamp01(double value)
    {
        if (value < 0.0)
            return 0.0;
        else if (value > 1.0)
            return 1.0;
        else
            return value;
    }


    public static float InverseLerp(double a, double b, double value)
    {
        if (a != b)
            return (float)Clamp01((value - a) / (b - a));
        else
            return 0f;
    }


    public static double DateTimeToUnixTimestamp(DateTime dateTime)
    {
        return (dateTime - new DateTime(1970, 1, 1)).TotalSeconds;
    }


    public static double UtcNow()
    {
        return DateTimeToUnixTimestamp(DateTime.UtcNow);
    }


    public static void DestroyChildren(Transform transform)
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
            GameObject.Destroy(transform.GetChild(i).gameObject);
    }
}

