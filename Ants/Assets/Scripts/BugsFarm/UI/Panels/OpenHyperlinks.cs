using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;


[RequireComponent(typeof(TextMeshProUGUI))]
public class OpenHyperlinks : MonoBehaviour, IPointerClickHandler
{
#pragma warning disable 0649

	[SerializeField] TextMeshProUGUI	_text;

#pragma warning restore 0649


	public void OnPointerClick( PointerEventData eventData )
	{
		int linkIndex				= TMP_TextUtilities.FindIntersectingLink( _text, Input.mousePosition, Camera.main );
		if (linkIndex == -1)
			return;

		TMP_LinkInfo linkInfo		= _text.textInfo.linkInfo[ linkIndex ];

		Application.OpenURL( linkInfo.GetLinkID() );
	}
}

