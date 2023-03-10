using UnityEngine;
using UnityEngine.EventSystems;
using Debug = UnityEngine.Debug;

public class Test : MonoBehaviour, IPointerClickHandler
{
    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log(gameObject.name);
    }
}
