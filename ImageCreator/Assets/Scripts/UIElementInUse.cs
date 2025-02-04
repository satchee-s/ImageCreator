using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UIElements;

public class UIElementInUse : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public static bool isInUse = false;
    public void OnPointerDown(PointerEventData eventData)
    {
        isInUse = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isInUse = false;
    }
}