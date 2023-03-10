using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
[RequireComponent(typeof(AspectRatioFitter))]
public class AspectRatioHelper : MonoBehaviour
{
    public Image image;
    public AspectRatioFitter aspectRatioFitter;

    private void OnEnable()
    {
        Refresh();
    }

    [ContextMenu("Refresh")]
    public void Refresh()
    {
        aspectRatioFitter.aspectRatio = (float)image.sprite.texture.width / (float)image.sprite.texture.height;
    }
    public void Reset()
    {
        image = GetComponent<Image>();
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
    }
}
