using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BrushSizeSlider1 : MonoBehaviour, IPointerUpHandler, IPointerDownHandler
{
    public Slider brushSlider;
    public bool isInUse;
    public void OnPointerDown(PointerEventData eventData)
    {
        isInUse = true;
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        isInUse = false;
    }
}
