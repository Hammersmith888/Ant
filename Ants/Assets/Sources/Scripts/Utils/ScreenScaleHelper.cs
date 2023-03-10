using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CanvasScaler))]
public class ScreenScaleHelper : MonoBehaviour
{
    public Vector2 MainScreenResolution = new Vector2(1080, 2160);
    public CanvasScaler CanvasScaler;
    private void Awake()
    {
        UpdateScaler();
    }

    private void UpdateScaler()
    {
#if UNITY_EDITOR
        var size = GetMainGameViewSize();
        CanvasScaler.scaleFactor = size.y / MainScreenResolution.y;
        return;
#endif

         CanvasScaler.scaleFactor = Screen.currentResolution.height / MainScreenResolution.y;
    }

    private void Reset()
    {
        CanvasScaler = GetComponent<CanvasScaler>();
    }

#if UNITY_EDITOR
    public static Vector2 GetMainGameViewSize()
    {
        System.Type T = System.Type.GetType("UnityEditor.GameView,UnityEditor");
        System.Reflection.MethodInfo GetSizeOfMainGameView = T.GetMethod("GetSizeOfMainGameView", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
        System.Object Res = GetSizeOfMainGameView.Invoke(null, null);
        return (Vector2)Res;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            UpdateScaler();
        }
    }
#endif
}