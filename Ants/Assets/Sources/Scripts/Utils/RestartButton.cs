using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class RestartButton : MonoBehaviour
{
    public bool withAd;
    public void OnClick()
    {
        Time.timeScale = 1;
        if (withAd)
        {
            AdManager.Instance.ShowInter();
        }
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(OnClick);
    }
}