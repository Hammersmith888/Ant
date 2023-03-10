using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bootstrap : MonoBehaviour
{
    public ScreenOrientation screenOrientation = ScreenOrientation.Portrait;
    public static bool IsInit = false;
    private void Awake()
    {
        if (IsInit)
        {
            Destroy(gameObject);
            return;
        }

        Application.targetFrameRate = 60;
        Screen.orientation = screenOrientation;

        IsInit = true;
    }
}
