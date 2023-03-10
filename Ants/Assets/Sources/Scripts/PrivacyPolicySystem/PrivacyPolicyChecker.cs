using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_IOS
// Include the IosSupport namespace if running on iOS:
using Unity.Advertisement.IosSupport;
#endif
public class PrivacyPolicyChecker : MonoBehaviour
{
    private const string KEY = "PRIVACY_POLICY_ACTIVE";
    public string fileName = "privacy_policy.html";

    private string Path => UniWebViewHelper.StreamingAssetURLForPath("Files/" + fileName);
    public UniWebView webView;

    private void Start()
    {
#if UNITY_IOS
        // Check the user's consent status.
        // If the status is undetermined, display the request request:
        if (ATTrackingStatusBinding.GetAuthorizationTrackingStatus() == ATTrackingStatusBinding.AuthorizationTrackingStatus.NOT_DETERMINED)
        {
            ATTrackingStatusBinding.RequestAuthorizationTracking();
        }
#endif

        if (PlayerPrefs.HasKey(KEY) == false)
        {
            webView.Load(Path);
            webView.SetBackButtonEnabled(false);
            webView.Show();

            webView.OnMessageReceived += (view, message) => {
                webView.Hide();
                webView.gameObject.SetActive(false);
                PlayerPrefs.SetInt(KEY, 1);
            };
        } else
        {
            webView.gameObject.SetActive(false);
        }
    }
}
