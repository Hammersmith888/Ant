using UnityEngine;
using UnityEngine.EventSystems;

public class ClickNotifier : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log("Pressed on me");
    }
}
