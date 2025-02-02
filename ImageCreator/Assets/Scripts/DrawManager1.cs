using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DrawManager1 : MonoBehaviour
{
    #region ShaderVariables
    [SerializeField] ComputeShader shader;
    [SerializeField] BrushSizeSlider1 slider;
    [SerializeField] Color bgColor;
    [SerializeField] Color brushColor;
    [SerializeField, Range(0.01f, 1)] float strokeSmoothingInterval = 0.1f;
    #endregion

    float brushSize;
    RenderTexture canvasTexture;
    Vector4 previousMousePos;

    private void Start()
    {
        slider.brushSlider.SetValueWithoutNotify(brushSize);

        canvasTexture = new RenderTexture(Screen.width, Screen.height, 24);
        canvasTexture.filterMode = FilterMode.Point;
        canvasTexture.enableRandomWrite = true;
        canvasTexture.Create();

        int initBackgroundKernel = shader.FindKernel("InitBackground");
        shader.SetVector("_BackgroundColor", bgColor);
        shader.SetFloat("_CanvasWidth", canvasTexture.width);
        shader.SetFloat("_CanvasHeight", canvasTexture.height);
        shader.SetTexture(initBackgroundKernel, "Canvas", canvasTexture);

        shader.GetKernelThreadGroupSizes(initBackgroundKernel, out uint xGroupSize, out uint yGroupSize, out _);
        shader.Dispatch(initBackgroundKernel, Mathf.CeilToInt (canvasTexture.width / (float)xGroupSize), 
                        Mathf.CeilToInt(canvasTexture.height / (float)yGroupSize), 1);
        //shader.Dispatch(initBackgroundKernel, Mathf.CeilToInt
        //        (canvasTexture.width / (float)xGroupSize), canvasTexture.height / 8, 1);

        previousMousePos = Input.mousePosition;
    }

    private void Update()
    {
        if (!slider.isInUse && Input.GetMouseButtonDown(0))
        {
            int updateKernel = shader.FindKernel("Update");

            shader.SetTexture(updateKernel, "Canvas", canvasTexture);
            shader.SetVector("_PreviousMousePosition", previousMousePos);
            shader.SetVector("_MousePosition", Input.mousePosition);
            shader.SetBool("_MouseDown", Input.GetMouseButton(0));
            shader.SetFloat("_BrushSize", brushSize);
            shader.SetVector("_BrushColour", brushColor);
            shader.SetFloat("_StrokeSmoothingInterval", strokeSmoothingInterval);

            shader.GetKernelThreadGroupSizes(updateKernel, out uint xGroupSize,out uint yGroupSize, out _);
            shader.Dispatch(updateKernel, Mathf.CeilToInt(canvasTexture.width / (float)xGroupSize),
                            Mathf.CeilToInt(canvasTexture.height / (float)yGroupSize), 1);
        }
        previousMousePos = Input.mousePosition;
    }

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(canvasTexture, destination);
    }

    public void OnBrushSizeChanged()
    {
        brushSize = slider.brushSlider.value;
    }
}
