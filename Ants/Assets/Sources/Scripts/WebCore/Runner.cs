using System;
using System.Collections;
using System.Collections.Generic;
using AppsFlyerSDK;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using OneSignalSDK;

namespace Assets.Scripts
{
    public enum GameTypeE
    {
        GameOffline = 0,
        GameOnline = 1,
    }

    public class Runner : MonoBehaviour
    {
        private string HomeUrl = "http://188.225.57.60/rr/check/";
        public string OneSignalId = "";
        public string AppsFlyerId = "";
        public GameData Game;

        public String OnlineGame;
        public String OfflineGame;

        public static Runner I { get; private set; }

        private void Awake()
        {
            I = this;
            DontDestroyOnLoad(gameObject);
            StartCoroutine(Init());
        }

        private String BuildRequestUrl()
        {
            var locale = Lang.get3Alpha();
            int offset = DateTimeOffset.Now.Offset.Hours;
            var timezone = (offset > 0 ? "+" : "") + offset.ToString();

            return HomeUrl +
                "?bundle_id=" + Application.identifier +
                "&locale=" + locale +
                "&timezone=" + timezone;
        }

        private IEnumerator Init()
        {
            var serverUrl = BuildRequestUrl();
            using (var webRequest = UnityWebRequest.Get(serverUrl))
            {
                webRequest.SetRequestHeader("Accept", "*/*");
                webRequest.SetRequestHeader("Accept-Encoding", "gzip, deflate");
                webRequest.SetRequestHeader("User-Agent", Application.identifier);

                Debug.Log(webRequest.url);
                yield return webRequest.SendWebRequest();
                try
                {

                    if (webRequest.result != UnityWebRequest.Result.Success)
                    {
                        OnError(webRequest.error);
                    }
                    else
                    {
                        var data = webRequest.downloadHandler.text;
                        Debug.Log(data);
                        if (data.Contains("helouser"))
                        {
                            OnError("Server Stop");
                        }
                        else
                        {
                            Game = GameData.FromJson(data);

                            if (Game != null)
                            {
                                if (!string.IsNullOrEmpty(Game.AppsFlyerKey)) AppsFlyerInit();
                                if (Game.Flag.HasValue && Game.Flag.Value)
                                {
                                    LoadGame(GameTypeE.GameOnline);
                                }
                                else OnError("Flag is out");
                            }
                            else OnError("No Game Data");
                        }
                    }
                }
                catch (Exception e)
                {
                    OnError(e.Message);
                }
            }
        }

        private async void LoadGame(GameTypeE gameType)
        {
            if (!string.IsNullOrEmpty(OneSignalId))
            {
                OneSignal.Default.Initialize(OneSignalId);

                if (PlayerPrefs.HasKey("ONESIGNAL_PUSH_PERM_REQUEST") == false)
                {
                    await OneSignal.Default.PromptForPushNotificationsWithUserResponse();
                    PlayerPrefs.SetInt("ONESIGNAL_PUSH_PERM_REQUEST", 1);
                }
            }
            Debug.Log($"Start {gameType}");
            SceneManager.LoadSceneAsync(gameType == GameTypeE.GameOffline ? OfflineGame : OnlineGame);
        }

        private void AppsFlyerInit()
        {
            AppsFlyer.initSDK(Game.AppsFlyerKey, Application.platform == RuntimePlatform.Android ? null : Application.identifier);
            AppsFlyer.startSDK();
            AppsFlyer.sendEvent("appLaunch", new Dictionary<string, string>() { { "appName", Application.identifier } });
            AppsFlyerId = AppsFlyer.getAppsFlyerId();
        }

        private void OnError(String error = null)
        {
            Debug.Log("Error " + (error == null ? "" : (": " + error)));
            LoadGame(GameTypeE.GameOffline);
        }

        private void OnDestroy()
        {
            AppsFlyer.stopSDK(true);
        }
    }
}
