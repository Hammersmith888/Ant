using UnityEngine;
using UnityEngine.UI;


/*
	https://forum.unity.com/threads/tint-multiple-targets-with-single-button.279820/#post-5092682
*/

     
public class MultiImageButton : Button
{
	protected override void DoStateTransition( SelectionState state, bool instant )
	{
		Color targetColor		=
									state == SelectionState.Disabled		? colors.disabledColor		:
									state == SelectionState.Highlighted		? colors.highlightedColor	:
									state == SelectionState.Normal			? colors.normalColor		:
									state == SelectionState.Pressed			? colors.pressedColor		:
									state == SelectionState.Selected		? colors.selectedColor		:
									Color.white
		;

     
		foreach (var graphic in GetComponentsInChildren< Graphic >())
			graphic.CrossFadeColor(
				targetColor,
				instant ? 0 : colors.fadeDuration,
				true,
				true
			);
	}
}

