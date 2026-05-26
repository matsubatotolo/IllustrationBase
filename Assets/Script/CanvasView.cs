using UnityEngine;
using UnityEngine.UI;

public class CanvasView : MonoBehaviour
{
    public DrawingCanvas canvas;
    public RawImage rawImage;

    void Start()
    {
        rawImage.texture = canvas.renderTexture;
    }
}