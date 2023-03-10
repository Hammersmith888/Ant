using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Advertisements;
public class AdManager : MonoBehaviour, IUnityAdsLoadListener, IUnityAdsShowListener
{
    public static AdManager Instance; private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        } else
        {
            Destroy(gameObject);
        }
    }

    public string androidId;
    public string iosId;
    private int count = 0;

    public string InterId
    {
        get
        {
            if (Application.platform == RuntimePlatform.Android)
            {
                return androidId;
            }
            else if (Application.platform == RuntimePlatform.IPhonePlayer)
            {
                return iosId;
            }
            else
            {
                return androidId;
            }
        }
    }

    public void ShowInter()
    {
        count -= 1;
        if(count <= 0)
        {
            Advertisement.Show(InterId, this);
            count = 6;
        }
    }

    public void Load()
    {
        Advertisement.Load(InterId, this);
    }

    public void OnUnityAdsAdLoaded(string placementId)
    {

    }

    public void OnUnityAdsFailedToLoad(string placementId, UnityAdsLoadError error, string message)
    {
        Load();
    }

    public void OnUnityAdsShowClick(string placementId)
    {

    }

    public void OnUnityAdsShowComplete(string placementId, UnityAdsShowCompletionState showCompletionState)
    {
        Load();
    }

    public void OnUnityAdsShowFailure(string placementId, UnityAdsShowError error, string message)
    {
        Load();
    }

    public void OnUnityAdsShowStart(string placementId)
    {

    }
}
