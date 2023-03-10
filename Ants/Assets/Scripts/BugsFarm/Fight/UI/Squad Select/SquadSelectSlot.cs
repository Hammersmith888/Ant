using UnityEngine;
using UnityEngine.EventSystems;


public class SquadSelectSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public static SquadSelectSlot	HoveredSlot		{ get; private set; }


	public void OnPointerEnter	( PointerEventData eventData )		=> HoveredSlot		= this;
	public void OnPointerExit	( PointerEventData eventData )		=> HoveredSlot		= null;
}

