using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] Color brushColor;
    [SerializeField] Slider brushSlider;

    float brushSize = 10f;
    float smoothingInterval = 0.01f;
    RenderTexture canvasTexture;
    Vector4 previousMousePos;

    void Start()
    {
        brushSlider.SetValueWithoutNotify(brushSize);

        canvasTexture = new RenderTexture(Screen.width, Screen.height, 24);
        canvasTexture.filterMode = FilterMode.Point;
        canvasTexture.enableRandomWrite = true;
        canvasTexture.Create();

        shader.SetFloat("_CanvasWidth", canvasTexture.width);
        shader.SetFloat("_CanvasHeight", canvasTexture.height);
        ClearCanvas();
        previousMousePos = Input.mousePosition;
    }

    void Update()
    {
        if (!UIElementInUse.isInUse && Input.GetMouseButton(0))
        {
            int updateKernel = shader.FindKernel("Update");
            shader.SetVector("_PreviousMousePosition", previousMousePos);
            shader.SetVector("_MousePosition", Input.mousePosition);
            shader.SetBool("_MouseDown", Input.GetMouseButton(0));
            shader.SetFloat("_BrushSize", brushSize);
            shader.SetVector("_BrushColour", brushColor);
            shader.SetFloat("_StrokeSmoothingInterval", smoothingInterval);
            shader.SetTexture(updateKernel, "_Canvas", canvasTexture);

            shader.GetKernelThreadGroupSizes(updateKernel,
                out uint xGroupSize, out uint yGroupSize, out _);
            shader.Dispatch(updateKernel,
                Mathf.CeilToInt(canvasTexture.width / (float) xGroupSize),
                Mathf.CeilToInt(canvasTexture.height / (float) yGroupSize),
                1);
        }
        previousMousePos = Input.mousePosition;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(canvasTexture, dest);
    }

    public void OnBrushSizeChanged()
    {
        brushSize = brushSlider.value;
    }

    public void ClearCanvas()
    {
        int initBackgroundKernel = shader.FindKernel("InitBackground");
        shader.SetTexture(initBackgroundKernel, "_Canvas", canvasTexture);
        shader.GetKernelThreadGroupSizes(initBackgroundKernel,
            out uint xGroupSize, out uint yGroupSize, out _);
        shader.Dispatch(initBackgroundKernel,
            Mathf.CeilToInt(canvasTexture.width / (float)xGroupSize),
            Mathf.CeilToInt(canvasTexture.height / (float)yGroupSize),
            1);
    }

    public void SaveImage()
    {
        RenderTexture.active = canvasTexture;
        Texture2D texture = new (canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, true);
        texture.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
        RenderTexture.active = null;
        byte[]  bytes = texture.EncodeToPNG();
        //string path = Application.streamingAssetsPath;
        string path = "Assets/Images/image.png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("saved to " + path);
    }
}