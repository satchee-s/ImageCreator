using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChangeBrushColor : MonoBehaviour
{
    [SerializeField] Color col;
    [SerializeField] DrawManager drawManager;

    public void ChangeColor()
    {
        drawManager.brushColor = col;
    }
}
