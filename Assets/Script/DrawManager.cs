using UnityEngine;
using UnityEngine.UI;

public class DrawManager : MonoBehaviour
{
    public DrawingCanvas canvas;
    public Brush brush;

    public RectTransform canvasRect;
    public Camera uiCamera;

    public void DrawLine(Vector2 from, Vector2 to, float pressure)
    {
        //Debug.Log("DrawLine: from + " + from + " , to = " + to);

        Vector2 uvFrom = ScreenToUV(from);
        Vector2 uvTo = ScreenToUV(to);

        //Debug.Log("DrawLine: uvFrom + " + uvFrom + " , uvTo = " + uvTo);

        float dist = Vector2.Distance(uvFrom, uvTo);
        //int steps = Mathf.CeilToInt(dist * 200);

        //for (int i = 0; i <= steps; i++)
        //{
        //    float t = i / (float)steps;
        //    Vector2 uv = Vector2.Lerp(uvFrom, uvTo, t);

        //    Debug.Log("DrawLine: uv + " + uv + " , pressure = " + pressure);            
        //    brush.Apply(uv, pressure, canvas.renderTexture);
        //}
        if (dist < 0.0001f)
        {
            brush.Apply(uvFrom, pressure, canvas.renderTexture);

            Debug.Log("DrawLine: uvFrom + " + uvFrom + " , pressure = " + pressure);

            return;
        }

        int steps = Mathf.CeilToInt(dist * 200);

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;

            Vector2 uv = Vector2.Lerp(uvFrom, uvTo, t);

            uv.x = Mathf.Clamp01(uv.x);
            uv.y = Mathf.Clamp01(uv.y);

            Debug.Log("DrawLine: uv + " + uv + " , pressure = " + pressure);

            brush.Apply(uv, pressure, canvas.renderTexture);
        }
    }

    Vector2 ScreenToUV(Vector2 screenPos)
    {
        //RectTransformUtility.ScreenPointToLocalPointInRectangle(
        //    canvasRect,
        //    screenPos,
        //    uiCamera,
        //    out Vector2 local);

        //Rect rect = canvasRect.rect;

        //float u = (local.x - rect.x) / rect.width;
        //float v = (local.y - rect.y) / rect.height;

        //return new Vector2(u, v);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
        canvasRect,
        screenPos,
        null,
        out Vector2 local);

        Rect rect = canvasRect.rect;

        float u = Mathf.InverseLerp(rect.xMin, rect.xMax, local.x);
        float v = Mathf.InverseLerp(rect.yMin, rect.yMax, local.y);

        return new Vector2(u, v);
    }
}