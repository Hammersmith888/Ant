using UnityEngine;

public static class RectTransformExtensions
{
    public static void SetWidth(this RectTransform rectTransform, float width)
    {
        var newSize = rectTransform.sizeDelta;
        newSize.x = width;
        rectTransform.sizeDelta = newSize;
    }

    public static void SetHeight(this RectTransform rectTransform, float height)
    {
        var newSize = rectTransform.sizeDelta;
        newSize.y = height;
        rectTransform.sizeDelta = newSize;
    }


    public static void SetXPosition(this RectTransform rectTransform, float x)
    {
        var newPosition = rectTransform.anchoredPosition;
        newPosition.x = x;
        rectTransform.anchoredPosition = newPosition;
    }

    public static void SetYPosition(this RectTransform rectTransform, float y)
    {
        var newPosition = rectTransform.anchoredPosition;
        newPosition.y = y;
        rectTransform.anchoredPosition = newPosition;
    }
}