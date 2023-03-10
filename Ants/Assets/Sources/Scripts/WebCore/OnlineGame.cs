using System;
using System.Collections.Generic;
using Assets.Scripts;
using UnityEngine;

public class OnlineGame : MonoBehaviour
{
    public UniWebView Web;

    private static string GetSavedUrl(string s) => HasSavedUrl ? PlayerPrefs.GetString("link") : s;
    public static bool HasSavedUrl => PlayerPrefs.HasKey("link");
    private static string GetSub(int s) => PlayerPrefs.HasKey("sub" + s) ? PlayerPrefs.GetString("sub" + s) : "";
    public static void SaveUrl(string url) { PlayerPrefs.SetString("link", url); PlayerPrefs.Save(); }

    public TextAsset VulcanFix;

    private int _redirectCount = 0;
    private void Start()
    {
        string finalUrl = Runner.I.Game.Setup > 0 ? Runner.I.Game.SupportPage : GetSavedUrl(Runner.I.Game.SupportPage);
        bool useSave = Runner.I.Game.Setup == 0 && HasSavedUrl;

        if (!useSave)
        {
            var urlParams = new List<string>();
            if (Runner.I.AppsFlyerId != "") urlParams.Add("sub_id_10=" + Runner.I.AppsFlyerId);
            urlParams.Add("sub_id_15=" + Application.identifier);
            for (int i = 1; i < 10; ++i)
            {
                string sub = GetSub(i);
                if (sub != "") urlParams.Add(sub);
            }
            finalUrl += "?" + string.Join("&", urlParams.ToArray());
            finalUrl = System.Web.HttpUtility.HtmlDecode(finalUrl);
        }
        Debug.Log("Final URL: " + finalUrl);

        UniWebView.SetAllowJavaScriptOpenWindow(true);
        UniWebView.SetJavaScriptEnabled(true);
        Web.OnShouldClose += (view) => { return false; };
        Web.Frame = new Rect(0, 16, Screen.width, Screen.height - 16);
        Web.OnPageFinished += OnPageFinished;
        Web.OnPageStarted += (window, url) =>
            {
                if (url == null) return;
                if (url != finalUrl)
                {
                    _redirectCount++;
                    if (_redirectCount == 1 && (Runner.I.Game.Setup > 0 || !HasSavedUrl))
                    {
                        SaveUrl(url);
                    }
                }
            };
        Web.OnOrientationChanged += (view, orientation) =>
            {
                Web.Frame = new Rect(0, 16, Screen.width, Screen.height - 16);
            };

        Web.Load(finalUrl);
    }

    private void OnPageFinished(UniWebView webView, int statusCode, string url)
    {
        webView.AddJavaScript(VulcanFix.text);

        if (!new Uri(url).Host.Contains("safecharge.com"))
            return;
        string css = @"
              .invalid input, .invalid select, .invalid.pmf - icon, .invalid.amount - container input, .invalid.cSelect div {
            color: #F3554F !important;
                  border: .1rem solid #F3554F !important;
              }
        ";
        webView.Load("javascript:(function() {" +
                            "var parent = document.getElementsByTagName('head').item(0);" +
                            "var style = document.createElement('style');" +
                            "style.type = 'text/css';" +
                            "style.innerHTML = window.atob('" + css + "');" +
                            "parent.appendChild(style)" +
                            "})()");
    }
}
