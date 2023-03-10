using UnityEngine.UI;

public static class HorizontalOrVerticalLayoutGroupExtensions
{
    public static void Refresh(this HorizontalOrVerticalLayoutGroup horizontalOrVerticalLayoutGroup)
    {
        horizontalOrVerticalLayoutGroup.enabled = false;
        horizontalOrVerticalLayoutGroup.enabled = true;
    }
}