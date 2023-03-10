using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class SceneChangeButton : MonoBehaviour
{
    public string sceneName;
    public Button button;
    public bool withAd = false;
    private void Awake()
    {
        button.onClick.AddListener(OnClick);
    }

    [OPS.Obfuscator.Attribute.DoNotRenameAttribute]
    public void OnClick()
    {
        Time.timeScale = 1f;

        if (withAd)
        {
            AdManager.Instance.ShowInter();
        }

        SceneManager.LoadScene(sceneName);
    }

    private void Reset()
    {
        button = GetComponent<Button>();
    }
}
