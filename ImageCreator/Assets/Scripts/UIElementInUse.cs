using UnityEngine;
using UnityEngine.EventSystems;

public class UIElementInUse : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public static bool isInUse = false;
    
    public void OnPointerUp(PointerEventData eventData)
    {
        isInUse = false;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        isInUse = true;
    }
}