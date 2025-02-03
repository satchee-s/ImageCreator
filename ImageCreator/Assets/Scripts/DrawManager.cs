using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    [SerializeField] ComputeShader shader;
    [SerializeField] Slider brushSlider;
    [SerializeField] GameObject inputParent;

    public Color brushColor;
    float brushSize = 10f;
    float smoothingInterval = 0.01f;
    RenderTexture canvasTexture;
    Vector4 previousMousePos;
    Vector3 input;
    bool isPressed;
    bool touchDetected;

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
        if (!UIElementInUse.isInUse && (Input.GetMouseButton(0) || Input.touchCount > 0))
        {
            if (Input.GetMouseButton(0))
            {
                input = Input.mousePosition;
                touchDetected = false;
            }
            else
            {
                input = Input.GetTouch(0).position;
                touchDetected = true;
            }
            isPressed = true;

            int updateKernel = shader.FindKernel("Update");
            shader.SetVector("_PreviousMousePosition", previousMousePos);
            shader.SetVector("_MousePosition", input);
            shader.SetBool("_MouseDown", isPressed);
            shader.SetFloat("_BrushSize", brushSize);
            shader.SetVector("_BrushColour", brushColor);
            shader.SetFloat("_StrokeSmoothingInterval", smoothingInterval);
            shader.SetTexture(updateKernel, "_Canvas", canvasTexture);

            shader.GetKernelThreadGroupSizes(updateKernel,
                out uint xGroupSize, out uint yGroupSize, out _);
            shader.Dispatch(updateKernel,
                Mathf.CeilToInt(canvasTexture.width / (float)xGroupSize),
                Mathf.CeilToInt(canvasTexture.height / (float)yGroupSize),
                1);
        }
        if (touchDetected && Input.GetTouch(0).phase != TouchPhase.Ended)
            previousMousePos = Input.GetTouch(0).position;
        else
            previousMousePos = Input.mousePosition;
        isPressed = false;
    }

    void OnRenderImage(RenderTexture src, RenderTexture dest)
    {
        Graphics.Blit(canvasTexture, dest);
    }

    public void OnBrushSizeChanged(float newValue)
    {
        brushSize = newValue;
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

    public void SaveImage(string fileName)
    {
        RenderTexture.active = canvasTexture;
        Texture2D texture = new (canvasTexture.width, canvasTexture.height, TextureFormat.RGBA32, true);
        texture.ReadPixels(new Rect(0, 0, canvasTexture.width, canvasTexture.height), 0, 0);
        RenderTexture.active = null;
        byte[]  bytes = texture.EncodeToPNG();
        string path = $"{Application.streamingAssetsPath}/{fileName}.png";
        System.IO.File.WriteAllBytes(path, bytes);
        Debug.Log("saved to " + path);
        UIElementInUse.isInUse = false;
        inputParent.SetActive(false);
    }

    public void EnabelInputField()
    {
        UIElementInUse.isInUse = true;
        inputParent.SetActive(true);
    }
}