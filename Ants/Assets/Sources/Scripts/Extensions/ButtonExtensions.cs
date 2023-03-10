using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class ButtonExtensions
{
    public static void SetInteractable(this List<Button> buttons, bool isActive)
    {
        buttons.ForEach((bt) =>
        {
            bt.interactable = isActive;
        });
    }
}
