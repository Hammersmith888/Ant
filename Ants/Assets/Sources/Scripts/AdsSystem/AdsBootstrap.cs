using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;

public class AdsBootstrap : MonoBehaviour, IUnityAdsInitializationListener
{
    public static bool IsInit = false;

    public string androidId;
    public string iosId;
    public bool testMode;

    public string GameId
    {
        get
        {
            if(Application.platform == RuntimePlatform.Android)
            {
                return androidId;
            } else if(Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return iosId;
            } else
            {
                return androidId;
            }
        }
    }

    private void Awake()
    {
        if (IsInit)
        {
            return;
        }

        Init();
    }

    public void Init()
    {
        Advertisement.Initialize(GameId, testMode, this);
        IsInit = true;
    }
    public void OnInitializationComplete()
    {
        AdManager.Instance.Load();
    }

    public void OnInitializationFailed(UnityAdsInitializationError error, string message)
    {

    }
}
