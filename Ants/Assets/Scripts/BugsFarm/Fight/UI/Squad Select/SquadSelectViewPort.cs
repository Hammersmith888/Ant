using UnityEngine.EventSystems;


public class SquadSelectViewPort : MB_Singleton< SquadSelectViewPort >, IPointerEnterHandler, IPointerExitHandler
{
	public static bool	Hovered		{ get; private set; }


	public void OnPointerEnter	( PointerEventData eventData )		=> Hovered		= true;
	public void OnPointerExit	( PointerEventData eventData )		=> Hovered		= false;
}

